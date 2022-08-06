using Amazon.Runtime;

namespace VideoApplication.Shared.Storage;

public class AwsHttpClientFactory : HttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AwsHttpClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override HttpClient CreateHttpClient(IClientConfig clientConfig)
    {
        return _httpClientFactory.CreateClient("s3-storage");
    }
}