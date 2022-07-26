﻿using System.ComponentModel.DataAnnotations;
using System.IO.Pipelines;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Exceptions.Channels;
using VideoApplication.Api.Exceptions.Upload;
using VideoApplication.Api.Extensions;
using VideoApplication.Api.Helpers;
using VideoApplication.Api.Services;

namespace VideoApplication.Api.Controllers;

[Authorize]
[Route("api/upload")]
[ApiController]
public class UploadVideoController : ControllerBase
{
    private readonly ILogger<UploadVideoController> _logger;
    private readonly VideoApplicationDbContext _dbContext;
    private readonly StorageWrapper _storageWrapper;

    public UploadVideoController(ILogger<UploadVideoController> logger, VideoApplicationDbContext dbContext, StorageWrapper storageWrapper)
    {
        _logger = logger;
        _dbContext = dbContext;
        _storageWrapper = storageWrapper;
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
                FileName = request.FileName,
                Sha256Hash = request.Sha256Hash,
                FileSize = request.FileSize,
                Chunks = new List<UploadChunk>(),
                StorageUploadId = uploadId,
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
    public async Task<UploadChunkResponse> UploadChunk([FromQuery] Guid uploadId, [FromQuery] [Range(0, 10000)] int position, CancellationToken cancellationToken = default)
    {
        var userId = User.GetId();
        var uploadInfo = await _dbContext.Uploads
            .Where(u => u.Id == uploadId && u.Channel.OwnerId == userId)
            .Select(u => new Upload
            {
                Id = u.Id,
                ChannelId = u.ChannelId,
                StorageUploadId = u.StorageUploadId,
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (uploadInfo == null)
        {
            throw new UploadNotFoundException($"The specified upload with id '{uploadId}' was not found");
        }


        var uploadChunk =
            await _dbContext.UploadChunks.FirstOrDefaultAsync(c => c.UploadId == uploadId && c.Position == position,
                cancellationToken);

        if (uploadChunk == null)
        {
            uploadChunk = new UploadChunk()
            {
                Id = Guid.NewGuid(),
                Position = position,
                UploadId = uploadId,
            };
            _dbContext.UploadChunks.Add(uploadChunk);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        (uploadChunk.Sha256Hash, uploadChunk.StorageETag) = await DoUpload(uploadInfo, position, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateUploadChunkResponse(uploadChunk);
    }

    [HttpPost("finish-upload")]
    public async Task FinishUpload([FromBody] FinishUploadRequest request, CancellationToken cancellationToken = default)
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

        if (uploadInfo.Chunks.Any(c => c.StorageETag == null))
        {
            throw new UploadChunksNotFinishedException();
        }

        var storageKey = StorageStructureHelper.GetSourcePath(uploadInfo.ChannelId, uploadInfo.Id);
        var eTags = uploadInfo.Chunks
            .Select(c => new S3PartETag(c.StorageETag!, c.Position))
            .ToList();
        await _storageWrapper.FinishUpload(new FinishUploadContext(storageKey, uploadInfo.StorageUploadId, eTags), cancellationToken);


        _dbContext.Uploads.Remove(uploadInfo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task<(string sha256Hash, string storageETag)> DoUpload(Upload uploadInfo, int position, CancellationToken cancellationToken)
    {
        var uploadKey = StorageStructureHelper.GetSourcePath(uploadInfo.ChannelId, uploadInfo.Id);
        var uploadPipe = new Pipe(new PipeOptions(pauseWriterThreshold: 4096));
        await using var uploadReadStream = uploadPipe.Reader.AsStream();
        var uploadPartContext = new UploadPartContext(uploadKey, position, uploadInfo.StorageUploadId, uploadReadStream);
        var uploadTask = _storageWrapper.UploadPart(uploadPartContext, cancellationToken);
        await using var uploadWriteStream = uploadPipe.Writer.AsStream();
        await using var hashStream = new TeeReaderStream(Request.Body, uploadWriteStream);
        using var sha256 = SHA256.Create();
        var computeHashTask = sha256.ComputeHashAsync(hashStream, cancellationToken);
        var storageETag = await uploadTask;
        var sha256Hash = await computeHashTask;
        return (Convert.ToHexString(sha256Hash), storageETag);
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

public record UploadChunkResponse(int Position, string Sha256Hash);

public record FinishUploadRequest(Guid UploadId);