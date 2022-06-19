using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Channels;

public class NotChannelOwnerException : ChannelException
{
    public NotChannelOwnerException() : base("Current user is not the owner of the requested channel")
    {
    }

    protected override ErrorKind ErrorKind => ErrorKind.BadRequest;
    protected override DetailedChannelErrorCode DetailedErrorCode => DetailedChannelErrorCode.NotChannelOwner;
}