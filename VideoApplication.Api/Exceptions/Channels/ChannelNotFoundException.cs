using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Channels;

public class ChannelNotFoundException : ChannelException
{
    public ChannelNotFoundException(string channelSlug) : base($"Channel with identifier {channelSlug} was not found")
    {
    }

    protected override ErrorKind ErrorKind => ErrorKind.NotFound;
    protected override DetailedChannelErrorCode DetailedErrorCode => DetailedChannelErrorCode.ChannelNotFound;
}