using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Subscriptions;
using Rebus.Transport;
using Rebus.Transport.InMem;
using VideoApplication.Shared.Setup;
using VideoApplication.Shared.Storage;
using VideoApplication.Worker;

namespace VideoApplication.Api.Tests;

public abstract class IntegrationTestBase<T> : TestBase<T> where T : class
{
    private ServiceProvider? _workerServiceProvider;

    private readonly InMemNetwork _memNetwork = new(true);
    private readonly InMemorySubscriberStore _inMemorySubscriberStore = new();

    private ServiceProvider CreateWorkerServiceProvider(StorageSettings storageSettings)
    {
        var services = new ServiceCollection();

        AddTestLogging(services);
        services.AddWorkerServices();
        services.ConfigureRebus(new InMemoryRebusTransport(RouteName.Worker, _memNetwork, _inMemorySubscriberStore));
        services.AddSingleton<IHostApplicationLifetime, ApplicationLifetime>();

        ConfigureTestStorage(services, storageSettings.BucketName);

        return services.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }
    
    private class InMemoryRebusTransport : IRebusTransportConfiguration
    {
        private readonly InMemNetwork _memNetwork;
        private readonly InMemorySubscriberStore _inMemorySubscriberStore; 

        public InMemoryRebusTransport(RouteName routeName, InMemNetwork memNetwork, InMemorySubscriberStore inMemorySubscriberStore)
        {
            _memNetwork = memNetwork;
            _inMemorySubscriberStore = inMemorySubscriberStore;
            RouteName = routeName;
        }

        public RouteName RouteName { get; }
        public void ConfigureTransport(StandardConfigurer<ITransport> t)
        {
            t.UseInMemoryTransport(_memNetwork, RouteName);
            
            t.OtherService<ISubscriptionStorage>().StoreInMemory(_inMemorySubscriberStore);
        }
    }

    public override async Task Setup()
    {
        await base.Setup();

        var storageSettings = RootServiceProvider.GetRequiredService<IOptions<StorageSettings>>();
        
        _workerServiceProvider = CreateWorkerServiceProvider(storageSettings.Value);
        

        await StartHostedServices(_workerServiceProvider);
        await StartHostedServices(RootServiceProvider);
    }

    private async Task StartHostedServices(IServiceProvider serviceProvider)
    {
        var services = serviceProvider.GetServices<IHostedService>().ToList();
        foreach (var hostedService in services)
        {
            await hostedService.StartAsync(CancellationToken.None);
        }
        
        await services.OfType<BackgroundSubscriber>().Single().Done;
    }

    public override async Task Teardown()
    {
        if (_workerServiceProvider != null)
        {
            await _workerServiceProvider.DisposeAsync();
            _workerServiceProvider = null;
        }
        
        await base.Teardown();
    }

    protected override void ConfigureBus(ServiceCollection services)
    {
        services.ConfigureRebus(new InMemoryRebusTransport(RouteName.Api, _memNetwork, _inMemorySubscriberStore));
    }
}