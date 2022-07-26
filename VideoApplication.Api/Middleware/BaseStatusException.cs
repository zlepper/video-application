namespace VideoApplication.Api.Middleware;

public abstract class BaseStatusException : Exception
{
    protected abstract ErrorKind ErrorKind { get; }

    protected BaseStatusException(string message) : base(message)
    {
    }

    public abstract ErrorResponse GetError();
}

public abstract class BaseStatusException<TDetailedErrorCode> : BaseStatusException
    where TDetailedErrorCode : struct
{
    protected BaseStatusException(string message) : base(message)
    {
    }
    
    protected abstract TDetailedErrorCode DetailedErrorCode { get; }
    
    
    protected virtual DetailedErrorResponse CreateDetailedError()
    {
        return new DetailedErrorResponse();
    }

    public override ErrorResponse GetError()
    {
        var res = CreateDetailedError();
        res.DetailedErrorCode = DetailedErrorCode;
        res.Error = ErrorKind;
        res.Message = Message;
        return res;
    }

    protected record DetailedErrorResponse : ErrorResponse
    {
        public TDetailedErrorCode DetailedErrorCode { get; set; }
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