using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Auth;



public abstract class AuthException : BaseStatusException<DetailedAuthErrorCode>
{
    protected AuthException(string message) : base(message)
    {
    }
}