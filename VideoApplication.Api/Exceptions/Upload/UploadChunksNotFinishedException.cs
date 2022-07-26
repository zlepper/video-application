using VideoApplication.Api.Middleware;

namespace VideoApplication.Api.Exceptions.Upload;

public class UploadChunksNotFinishedException : UploadException
{
    public UploadChunksNotFinishedException() : base("Some of the chunks that has been started to upload, has not been finished. Please finish those first, or abort the upload and re-uploading everything again.")
    {
    }

    protected override ErrorKind ErrorKind => ErrorKind.BadRequest;
    protected override DetailedUploadErrorCode DetailedErrorCode => DetailedUploadErrorCode.UploadChunksNotFinished;
}