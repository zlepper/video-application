using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace VideoApplication.Shared.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configurationRoot)
    {
        services.Configure<StorageSettings>(configurationRoot.GetSection("Storage"));

        services.AddSingleton<HttpClientFactory, AwsHttpClientFactory>();
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<HttpClientFactory>();
            
            var storageConfig = sp.GetRequiredService<IOptions<StorageSettings>>();

            var awsCredentials = new BasicAWSCredentials(storageConfig.Value.AccessKey, storageConfig.Value.SecretKey);
            var config = new AmazonS3Config()
            {
                HttpClientFactory = httpClientFactory,
                ServiceURL = storageConfig.Value.ServiceUrl,
                ForcePathStyle = true,
                RegionEndpoint = RegionEndpoint.USEast1
            };
            return new AmazonS3Client(awsCredentials, config);
        });

        services.AddScoped<StorageWrapper>();


        return services;
    }
}