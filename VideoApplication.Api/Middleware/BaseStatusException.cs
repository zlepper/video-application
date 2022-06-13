namespace VideoApplication.Api.Middleware;

public abstract class BaseStatusException : Exception
{
    protected abstract ErrorKind ErrorKind { get; }

    protected BaseStatusException(string message) : base(message)
    {
    }

    protected abstract ErrorResponse CreateErrorResponse();
    
    public ErrorResponse GetError()
    {
        var res = CreateErrorResponse();
        res.Error = ErrorKind;
        res.Message = Message;
        return res;
    }
}

public abstract record ErrorResponse
{
    public ErrorKind Error { get; set; }
    public string Message { get; set; } = null!;
}

public enum ErrorKind
{
    BadRequest = 400,
    Conflict = 409,
    NotFound = 404,
    InternalServerError = 500,
}