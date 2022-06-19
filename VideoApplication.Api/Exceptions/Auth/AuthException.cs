using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Auth;



public abstract class AuthException : BaseStatusException
{
    
    
    protected AuthException(string message) : base(message)
    {
    }

    protected abstract DetailedAuthErrorCode DetailedErrorCode { get; }
    protected abstract AuthErrorResponse CreateAuthError();
    
    protected override ErrorResponse CreateErrorResponse()
    {
        var res = CreateAuthError();
        res.DetailedErrorCode = DetailedErrorCode;
        return res;
    }

    protected record AuthErrorResponse : ErrorResponse
    {
        public DetailedAuthErrorCode DetailedErrorCode { get; set; }
    }
}