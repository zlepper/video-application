using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Auth;

public class EmailAlreadyInUseException : AuthException
{
    protected override ErrorKind ErrorKind => ErrorKind.Conflict;
    private readonly string _email;
    
    public EmailAlreadyInUseException(string email) : base($"Email address '{email}' is already in use")
    {
        _email = email;
    }

    protected override DetailedAuthErrorCode DetailedErrorCode => DetailedAuthErrorCode.EmailAlreadyInUse;
    protected override AuthErrorResponse CreateAuthError()
    {
        return new EmailAlreadyInUseErrorResponse(_email);
    }

    private record EmailAlreadyInUseErrorResponse(string Email) : AuthErrorResponse;
}