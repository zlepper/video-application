using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Auth;

public class InvalidEmailOrPasswordException : AuthException
{
    public InvalidEmailOrPasswordException() : base("Invalid username or password")
    {
    }

    protected override ErrorKind ErrorKind => ErrorKind.BadRequest;
    protected override DetailedAuthErrorCode DetailedErrorCode => DetailedAuthErrorCode.InvalidEmailOrPassword;
}