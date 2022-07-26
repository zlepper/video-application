namespace VideoApplication.Api.Services;

public class StorageSettings
{
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;

    public Uri ServiceUrl { get; set; } = null!;

    public string BucketName { get; set; } = null!;
}