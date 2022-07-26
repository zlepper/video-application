using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Channels;

public abstract class ChannelException : BaseStatusException<DetailedChannelErrorCode>
{
    protected ChannelException(string message) : base(message)
    {
    }
}