using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using VideoApplication.Api.Models;

namespace VideoApplication.Api.Services;

public class StorageWrapper
{
    private readonly IAmazonS3 _s3Client;
    private readonly IOptionsSnapshot<StorageSettings> _settings;

    private string bucketName => _settings.Value.BucketName;

    public StorageWrapper(IAmazonS3 s3Client, IOptionsSnapshot<StorageSettings> settings)
    {
        _s3Client = s3Client;
        _settings = settings;
    }

    public async Task<string> InitiateUpload(string key, CancellationToken cancellationToken)
    {
        var initiateResponse = await _s3Client.InitiateMultipartUploadAsync(bucketName, key, cancellationToken);
        return initiateResponse.UploadId;
    }

    public async Task<string> UploadPart(UploadPartContext context, CancellationToken cancellationToken)
    {
        var partResponse = await _s3Client.UploadPartAsync(new UploadPartRequest()
        {
            BucketName = bucketName,
            Key = context.Key,
            UploadId = context.UploadId,
            PartNumber = context.partNumber,
            InputStream = context.content,
            ChecksumSHA256 = Convert.ToBase64String(Convert.FromHexString(context.Sha256))
        }, cancellationToken);

        
        return partResponse.ETag;
    }

    public async Task FinishUpload(FinishUploadContext context, CancellationToken cancellationToken)
    {
        var partETags = context.PartETags
            .OrderBy(t => t.PartNumber)
            .Select(tag => new PartETag(tag.PartNumber, tag.ETag))
            .ToList();
        
        await _s3Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest()
        {
            UploadId = context.UploadId,
            Key = context.Key,
            BucketName = bucketName,
            PartETags = partETags,
        }, cancellationToken);
    }

    public async Task<CancelUploadResult> CancelUpload(string uploadId, string key, CancellationToken cancellationToken)
    {
        await _s3Client.AbortMultipartUploadAsync(bucketName, key, uploadId, cancellationToken);

        var parts = await _s3Client.ListPartsAsync(bucketName, key, uploadId, cancellationToken);
        if (parts.Parts.Count == 0)
        {
            return CancelUploadResult.Cancelled;
        }

        return CancelUploadResult.MoreToGo;
    }

    public async Task<Stream> OpenReadStream(string key, CancellationToken cancellationToken)
    {
        return await _s3Client.GetObjectStreamAsync(bucketName, key, null, cancellationToken: cancellationToken);
    }
}

public enum CancelUploadResult
{
    Cancelled,
    MoreToGo
}

public record struct UploadPartContext(string Key, int partNumber, string UploadId, Stream content, string Sha256);
public record struct FinishUploadContext(string Key, string UploadId, List<S3PartETag> PartETags);

public record struct S3PartETag(string ETag, int PartNumber);