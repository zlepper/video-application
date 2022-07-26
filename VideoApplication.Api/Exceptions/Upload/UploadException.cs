using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Upload;

public abstract class UploadException : BaseStatusException<DetailedUploadErrorCode>
{
    protected UploadException(string message) : base(message)
    {
    }
}