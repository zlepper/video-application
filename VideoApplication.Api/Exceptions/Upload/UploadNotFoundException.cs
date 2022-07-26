using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Upload;

public class UploadNotFoundException : UploadException
{
    public UploadNotFoundException(string message) : base(message)
    {
    }

    protected override ErrorKind ErrorKind => ErrorKind.NotFound;
    protected override DetailedUploadErrorCode DetailedErrorCode => DetailedUploadErrorCode.UploadNotFound;
}