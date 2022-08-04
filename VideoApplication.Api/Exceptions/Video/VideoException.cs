using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Video;

public abstract class VideoException : BaseStatusException<DetailedVideoErrorCode>
{
    protected VideoException(string message) : base(message)
    {
    }
}