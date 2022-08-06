using Rebus.Bus;
using Rebus.Handlers;
using VideoApplication.Api.Shared.Events;
using VideoApplication.Shared.Storage;
using VideoApplication.Worker.Ffmpeg;
using VideoApplication.Worker.Shared.Events;

namespace VideoApplication.Worker.Handlers;

public class TranscodeVideoHandler : IHandleMessages<VideoUploadFinished>, IHandleMessages<VideoTranscodingsIdentified>
{
    private readonly ILogger<TranscodeVideoHandler> _logger;
    private readonly IBus _bus;
    private readonly FfmpegConverter _ffmpegConverter;
    private readonly TempFileProvider _tempFileProvider;
    private readonly StorageWrapper _storage;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly FfprobeWrapper _ffprobe;

    
    private static readonly IReadOnlyList<int> Resolutions = new[]
    {
        480, 720, 1080, 1440, 2160, 4320
    };

    private static readonly IReadOnlyList<int> LowFpsResolutions = new[]
    {
        480,
    };

    private const int LowFpsThreshold = 30;
    
    public TranscodeVideoHandler(ILogger<TranscodeVideoHandler> logger, IBus bus, FfmpegConverter ffmpegConverter, TempFileProvider tempFileProvider, StorageWrapper storage, IHostApplicationLifetime lifetime, FfprobeWrapper ffprobe)
    {
        _logger = logger;
        _bus = bus;
        _ffmpegConverter = ffmpegConverter;
        _tempFileProvider = tempFileProvider;
        _storage = storage;
        _lifetime = lifetime;
        _ffprobe = ffprobe;
    }

    public async Task Handle(VideoUploadFinished message)
    {
        var cancellationToken = _lifetime.ApplicationStopping;
        var sourceDownloadPath = await DownloadSourceFile(message.ChannelId, message.VideoId, message.OriginalFileExtension, cancellationToken);

        var sourceFormats = await _ffprobe.GetVideoInformation(sourceDownloadPath, cancellationToken);

        var transcodingsToCreate = new List<QueuedTranscoding>();

        var audioStreams = sourceFormats.Streams.Where(s => s.StreamType == StreamType.Audio).ToList();
        foreach (var audioStream in audioStreams)
        {
            transcodingsToCreate.Add(new QueuedAudioTranscoding( audioStream.Title, audioStream.StreamIndex));
        }

        var maxHeight = sourceFormats.Streams.Max(s => s.Height);
        var maxFrameRate = sourceFormats.Streams.Max(s => s.FrameRate);

        var heights = Resolutions.TakeWhile(r => r <= maxHeight);
        
        foreach (var height in heights)
        {
            var frameRate = maxFrameRate;
            if (LowFpsResolutions.Contains(height) && frameRate > LowFpsThreshold)
            {
                frameRate /= 2;
            }
            transcodingsToCreate.Add(new QueuedVideoTranscoding(height, frameRate));
        }
        
        await _bus.Publish(new VideoTranscodingsIdentified(message.ChannelId, message.VideoId, message.OriginalFileExtension, sourceFormats.Duration, transcodingsToCreate));
    }
    
    public async Task Handle(VideoTranscodingsIdentified message)
    {
        var cancellationToken = _lifetime.ApplicationStopping;
        var sourceDownloadPath = await DownloadSourceFile(message.ChannelId, message.VideoId, message.OriginalFileExtension, cancellationToken);
        var outputDirectoryPath = await _ffmpegConverter.Convert(message.VideoId, sourceDownloadPath, message.Transcodings, cancellationToken);

        var files = Directory.EnumerateFiles(outputDirectoryPath, "*.*", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            var relativeName = file[(outputDirectoryPath.Length + 1)..];
            
            var storageKey =
                StorageStructureHelper.GetVideoStreamPath(message.ChannelId, message.VideoId, relativeName);

            await using var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            await _storage.Upload(storageKey, fileStream, cancellationToken);
        }

        await _bus.Publish(new VideoTranscodingFinished(message.ChannelId, message.VideoId));
    }
    

    private async Task<string> DownloadSourceFile(Guid channelId, Guid videoId, string originalFileExtension, CancellationToken cancellationToken)
    {
        var sourceStoragePath = StorageStructureHelper.GetSourcePath(channelId, videoId);

        var sourceDownloadPath = _tempFileProvider.GetTempFileName(originalFileExtension);

        await using var downloadStream = await _storage.OpenReadStream(sourceStoragePath, cancellationToken);
        await using var fileStream = File.Open(sourceDownloadPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await downloadStream.CopyToAsync(fileStream, cancellationToken);
        return sourceDownloadPath;
    }

}