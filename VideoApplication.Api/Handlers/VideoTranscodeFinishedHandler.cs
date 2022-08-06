using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Exceptions.Video;
using VideoApplication.Worker.Shared.Events;

namespace VideoApplication.Api.Handlers;

public class VideoTranscodeFinishedHandler : IHandleMessages<VideoTranscodingFinished>, IHandleMessages<VideoTranscodingsIdentified>, IHandleMessages<VideoTranscodeProgress>
{
    private readonly ILogger<VideoTranscodeFinishedHandler> _logger;
    private readonly VideoApplicationDbContext _context;

    public VideoTranscodeFinishedHandler(ILogger<VideoTranscodeFinishedHandler> logger, VideoApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task Handle(VideoTranscodingFinished message)
    {
        _logger.LogInformation("video transcoding finished: {@Message}", message);

        var video = await GetVideo(message.VideoId);

        video.ProcessingState = VideoProcessingState.Ready;
        video.ProcessedDuration = video.Duration;

        await _context.SaveChangesAsync();
    }

    public async Task Handle(VideoTranscodingsIdentified message)
    {
        var video = await GetVideo(message.VideoId);

        video.Duration = message.Duration;

        var audioTracks = message.Transcodings.OfType<QueuedAudioTranscoding>().Select(t => new VideoAudioTrack()
        {
            Id = Guid.NewGuid(),
            Name = t.Name,
            VideoId = message.VideoId,
            Index = t.StreamIndex
        });
        
        _context.VideoAudioTracks.AddRange(audioTracks);

        var videoTracks = message.Transcodings.OfType<QueuedVideoTranscoding>().Select(t => new VideoVideoTrack()
        {
            Id = Guid.NewGuid(),
            Height = t.Height,
            FrameRate = t.FrameRate,
            VideoId = message.VideoId
        });
        
        _context.VideoVideoTracks.AddRange(videoTracks);

        await _context.SaveChangesAsync();
    }

    public async Task Handle(VideoTranscodeProgress message)
    {
        var video = await GetVideo(message.VideoId);

        video.ProcessedDuration = message.Time;

        await _context.SaveChangesAsync();
    }
    
    
    private async Task<Video> GetVideo(Guid videoId)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId);

        if (video != null) return video;
        
        _logger.LogError("Could not find video with id '{@VideoId}'", videoId);
        throw new VideoNotFoundException(videoId);
    }

}