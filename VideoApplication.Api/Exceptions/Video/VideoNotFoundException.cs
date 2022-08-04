using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Video;

public class VideoNotFoundException : VideoException
{
    public VideoNotFoundException(Guid videoId) : base($"Video with id '{videoId}' was not found")
    {
    }

    protected override ErrorKind ErrorKind => ErrorKind.NotFound;
    protected override DetailedVideoErrorCode DetailedErrorCode => DetailedVideoErrorCode.NotFound;
}