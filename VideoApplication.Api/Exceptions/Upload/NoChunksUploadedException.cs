using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Upload;

public class NoChunksUploadedException : UploadException
{
    public NoChunksUploadedException() : base("No chunks has been uploaded")
    {
        
    }

    protected override ErrorKind ErrorKind => ErrorKind.BadRequest;
    protected override DetailedUploadErrorCode DetailedErrorCode => DetailedUploadErrorCode.NoChunksUploaded;
}