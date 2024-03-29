﻿using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Bus;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Exceptions.Channels;
using VideoApplication.Api.Exceptions.Upload;
using VideoApplication.Api.Extensions;
using VideoApplication.Api.Shared.Events;
using VideoApplication.Shared.Storage;

namespace VideoApplication.Api.Controllers;

[Authorize]
[Route("api/upload")]
[ApiController]
public class UploadVideoController : ControllerBase
{
    private readonly ILogger<UploadVideoController> _logger;
    private readonly VideoApplicationDbContext _dbContext;
    private readonly StorageWrapper _storageWrapper;
    private readonly IClock _clock;
    private readonly IBus _bus;

    public UploadVideoController(ILogger<UploadVideoController> logger, VideoApplicationDbContext dbContext, StorageWrapper storageWrapper, IClock clock, IBus bus)
    {
        _logger = logger;
        _dbContext = dbContext;
        _storageWrapper = storageWrapper;
        _clock = clock;
        _bus = bus;
    }


    [HttpPost("start-upload")]
    public async Task<StartVideoUploadResponse> StartVideoUpload([FromBody] StartVideoUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetId();
        _logger.LogInformation("Starting upload: {@Upload} by user '{@UserId}'", request, userId);

        var channel = await _dbContext.Channels.FirstOrDefaultAsync(c => c.Id == request.ChannelId, cancellationToken);

        if (channel == null)
        {
            _logger.LogWarning("Could not find channel '{@ChannelId}'", request.ChannelId);
            throw new ChannelNotFoundException(request.ChannelId.ToString());
        }

        if (channel.OwnerId != userId)
        {
            _logger.LogWarning("User tried to upload to channel they don't own");
            throw new NotChannelOwnerException();
        }
        
        _logger.LogInformation("Channel <=> owner validation passed, creating upload");
        
        var existingUpload = await _dbContext.Uploads
            .Include(u => u.Chunks)
            .FirstOrDefaultAsync(u => u.ChannelId == channel.Id && u.Sha256Hash == request.Sha256Hash, cancellationToken);

        if (existingUpload != null)
        {
            return CreateStartResponse(existingUpload);
        }

        try
        {
            var videoId = Guid.NewGuid();
            var sourcePath = StorageStructureHelper.GetSourcePath(channel.Id, videoId);

            var uploadId = await _storageWrapper.InitiateUpload(sourcePath, cancellationToken);
            var upload = new Upload()
            {
                Id = videoId,
                ChannelId = channel.Id,
                FileName = Path.GetFileName(request.FileName),
                Sha256Hash = request.Sha256Hash,
                FileSize = request.FileSize,
                Chunks = new List<UploadChunk>(),
                StorageUploadId = uploadId,
                UploadStartDate = _clock.GetCurrentInstant()
            };

            _dbContext.Uploads.Add(upload);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return CreateStartResponse(upload);
        }
        catch (DbUpdateException e) when (e.IsUniqueConstraintViolation())
        {
            _logger.LogWarning("Upload already exists, returning existing upload");
            existingUpload = await _dbContext.Uploads
                .Include(u => u.Chunks)
                .FirstAsync(u => u.ChannelId == channel.Id && u.Sha256Hash == request.Sha256Hash, cancellationToken);

            return CreateStartResponse(existingUpload);
        }
    }

    [HttpPost("upload-chunk")]
    public async Task<UploadChunkResponse> UploadChunk([FromForm]UploadChunkRequest request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetId();
        var uploadInfo = await _dbContext.Uploads
            .Where(u => u.Id == request.UploadId && u.Channel.OwnerId == userId)
            .Select(u => new Upload
            {
                Id = u.Id,
                ChannelId = u.ChannelId,
                StorageUploadId = u.StorageUploadId,
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (uploadInfo == null)
        {
            throw new UploadNotFoundException($"The specified upload with id '{request.UploadId}' was not found");
        }


        var uploadChunk = await _dbContext.UploadChunks
                .FirstOrDefaultAsync(c => c.UploadId == request.UploadId && c.Position == request.Position,
                cancellationToken);

        if (uploadChunk == null)
        {
            uploadChunk = new UploadChunk()
            {
                Id = Guid.NewGuid(),
                Position = request.Position,
                UploadId = request.UploadId,
            };
            _dbContext.UploadChunks.Add(uploadChunk);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        uploadChunk.Sha256Hash = await Sha256File(request.Chunk, cancellationToken);
        uploadChunk.StorageETag = await DoUpload(uploadInfo, request, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateUploadChunkResponse(uploadChunk);
    }

    private async Task<string> DoUpload(Upload uploadInfo, UploadChunkRequest request,
        CancellationToken cancellationToken)
    {
        var uploadKey = StorageStructureHelper.GetSourcePath(uploadInfo.ChannelId, uploadInfo.Id);
        await using var uploadStream = request.Chunk.OpenReadStream();
        var uploadPartContext = new UploadPartContext(uploadKey, request.Position, uploadInfo.StorageUploadId, uploadStream);
        return await _storageWrapper.UploadPart(uploadPartContext, cancellationToken);
    }

    private async Task<string> Sha256File(IFormFile file, CancellationToken cancellationToken)
    {
        await using var fileStream = file.OpenReadStream();
        using var hasher = SHA256.Create();
        var hash = await hasher.ComputeHashAsync(fileStream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    [HttpPost("finish-upload")]
    public async Task<FinishUploadResponse> FinishUpload([FromBody] FinishUploadRequest request, CancellationToken cancellationToken = default)
    {
        var userId = User.GetId();
        var uploadInfo = await _dbContext.Uploads
            .Include(u => u.Chunks)
            .Where(u => u.Id == request.UploadId && u.Channel.OwnerId == userId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (uploadInfo == null)
        {
            throw new UploadNotFoundException($"The specified upload with id '{request.UploadId}' was not found");
        }

        if (uploadInfo.Chunks.Count == 0)
        {
            throw new NoChunksUploadedException();
        }

        if (uploadInfo.Chunks.Any(c => c.StorageETag == null))
        {
            throw new UploadChunksNotFinishedException();
        }

        var storageKey = StorageStructureHelper.GetSourcePath(uploadInfo.ChannelId, uploadInfo.Id);
        var eTags = uploadInfo.Chunks
            .Select(c => new S3PartETag(c.StorageETag!, c.Position))
            .ToList();
        await _storageWrapper.FinishUpload(new FinishUploadContext(storageKey, uploadInfo.StorageUploadId, eTags), cancellationToken);

        var video = new Video()
        {
            Id = uploadInfo.Id,
            ChannelId = uploadInfo.ChannelId,
            OriginalFileName = uploadInfo.FileName,
            UploadDate = uploadInfo.UploadStartDate,
            Name = Path.GetFileNameWithoutExtension(uploadInfo.FileName),
        };

        _dbContext.Uploads.Remove(uploadInfo);
        _dbContext.Videos.Add(video);
        
        await _bus.Publish(new VideoUploadFinished(video.ChannelId, video.Id, Path.GetExtension(video.OriginalFileName).Substring(1)));
        
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return new FinishUploadResponse(video.Id);
    }

    [HttpGet("{channelSlug}")]
    public async Task<List<UploadItem>> GetCurrentUploads([FromRoute] string channelSlug, CancellationToken cancellationToken)
    {
        var userId = User.GetId();

        var channel = await _dbContext.Channels.FirstOrDefaultAsync(c => c.IdentifierName == channelSlug && c.OwnerId == userId, cancellationToken);
        if (channel == null)
        {
            throw new ChannelNotFoundException(channelSlug);
        }

        return await _dbContext.Uploads.Where(u => u.ChannelId == channel.Id)
            .Select(u => new UploadItem(u.Id, u.FileName, u.Sha256Hash))
            .ToListAsync(cancellationToken);
    }
    
    private StartVideoUploadResponse CreateStartResponse(Upload upload)
    {
        var chunks = upload.Chunks
            .Where(c => c.Sha256Hash != null)
            .Select(CreateUploadChunkResponse).ToList();
        return new StartVideoUploadResponse(upload.Id, chunks);
    }

    private static UploadChunkResponse CreateUploadChunkResponse(UploadChunk chunk)
    {
        return new UploadChunkResponse(chunk.Position, chunk.Sha256Hash!);
    }
}

public record StartVideoUploadRequest(string Sha256Hash, Guid ChannelId, [FileExtensions(Extensions = "mp4")] string FileName, long FileSize);

public record StartVideoUploadResponse(Guid UploadId, List<UploadChunkResponse> UploadedChunks);

public record UploadChunkRequest(Guid UploadId, [Range(0, 10000)] int Position, string chunkSha256Hash, IFormFile Chunk);

public record UploadChunkResponse(int Position, string Sha256Hash);

public record FinishUploadRequest(Guid UploadId);

public record FinishUploadResponse(Guid VideoId);

public record UploadItem(Guid UploadId, string FileName, string Sha256);



