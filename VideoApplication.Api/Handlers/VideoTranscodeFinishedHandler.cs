using Rebus.Handlers;
using VideoApplication.Worker.Shared.Events;

namespace VideoApplication.Api.Handlers;

public class VideoTranscodeFinishedHandler : IHandleMessages<VideoTranscodingFinished>
{
    private ILogger<VideoTranscodeFinishedHandler> _logger;


    public VideoTranscodeFinishedHandler(ILogger<VideoTranscodeFinishedHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(VideoTranscodingFinished message)
    {
        _logger.LogInformation("video transcoding finished entirely!!: {@Message}", message);
    }
}