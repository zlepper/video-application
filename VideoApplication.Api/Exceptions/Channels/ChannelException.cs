using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Channels;

public abstract class ChannelException : BaseStatusException
{
    protected ChannelException(string message) : base(message)
    {
    }
    
    protected abstract DetailedChannelErrorCode DetailedErrorCode { get; }

    protected virtual ChannelErrorResponse CreateChannelError()
    {
        return new ChannelErrorResponse();
    }

    protected override ErrorResponse CreateErrorResponse()
    {
        var res = CreateChannelError();
        res.DetailedErrorCode = DetailedErrorCode;
        return res;
    }

    protected record ChannelErrorResponse : ErrorResponse
    {
        public DetailedChannelErrorCode DetailedErrorCode { get; set; }
    }
}