using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Channels;

public class ChannelAlreadyExistsException : ChannelException
{
    public ChannelAlreadyExistsException() : base("A channel with the same identifier or display name already exists.")
    {
    }

    protected override ErrorKind ErrorKind => ErrorKind.Conflict;

    protected override DetailedChannelErrorCode DetailedErrorCode =>
        DetailedChannelErrorCode.ChannelWithSameNameAlreadyExists;
}