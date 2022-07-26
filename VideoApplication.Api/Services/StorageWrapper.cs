using Microsoft.Extensions.Options;
using Minio;
using VideoApplication.Api.Models;

namespace VideoApplication.Api.Services;

public class StorageWrapper
{
    private readonly MinioClient _minioClient;
    private readonly IOptionsSnapshot<StorageSettings> _settings;

    private string bucketName => _settings.Value.BucketName;

    public StorageWrapper(MinioClient minioClient, IOptionsSnapshot<StorageSettings> settings)
    {
        _minioClient = minioClient;
        _settings = settings;
    }

    public async Task UploadBlob(string key, Stream stream, CancellationToken cancellationToken)
    {
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithObjectSize(-1)
            .WithStreamData(stream)
            .WithBucket(bucketName)
            .WithObject(key), cancellationToken);
    }

    public async Task DeleteBlobs(List<string> keys, CancellationToken cancellationToken)
    {
        var args = new RemoveObjectsArgs()
            .WithBucket(bucketName)
            .WithObjects(keys);
        await _minioClient.RemoveObjectsAsync(args, cancellationToken);
    }
    
    public async Task<Stream> OpenReadStream(string key, CancellationToken cancellationToken)
    {
        var args = new SelectObjectContentArgs()
            .WithBucket(bucketName)
            .WithObject(key);
        var result = await _minioClient.SelectObjectContentAsync(args, cancellationToken);
        return result.Payload; 
    }

    public async Task CreateBucket(string name, CancellationToken cancellationToken = default)
    {
        var args = new MakeBucketArgs()
            .WithBucket(name);
        await _minioClient.MakeBucketAsync(args, cancellationToken);
    }

    public async Task DeleteBucket(string name, CancellationToken cancellationToken = default)
    {
        var args = new RemoveBucketArgs()
            .WithBucket(name);
        await _minioClient.RemoveBucketAsync(args, cancellationToken);
    }
}

public enum CancelUploadResult
{
    Cancelled,
    MoreToGo
}

public record struct UploadPartContext(string Key, int partNumber, string UploadId, Stream content);
public record struct FinishUploadContext(string Key, string UploadId, List<S3PartETag> PartETags);

public record struct S3PartETag(string ETag, int PartNumber);