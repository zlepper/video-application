using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using VideoApplication.Api.Shared;
using VideoApplication.Worker.Shared;

namespace VideoApplication.Shared.Setup;

public static class RebusSetup
{
    public static void ConfigureRebus(this IServiceCollection serviceCollection, IRebusTransportConfiguration transportConfiguration)
    {
        serviceCollection.AddRebus(c => c
            .Options(o =>
            {
                o.EnableDiagnosticSources();
                o.SimpleRetryStrategy($"{transportConfiguration.RouteName}.error");
                o.SetBusName(transportConfiguration.RouteName);
            })
            .Routing(r => r.TypeBased()
                .MapAssemblyOf<VideoWorkerShared>(RouteName.Worker)
                .MapAssemblyOf<VideoApiShared>(RouteName.Api))
            .Transport(transportConfiguration.ConfigureTransport));
        
        serviceCollection.AddHostedService<BackgroundSubscriber>();
    }

    public static IRebusTransportConfiguration UseRebusRabbitMqTransport(this IConfiguration configuration, RouteName routeName)
    {
        var rebusSettings = configuration.GetSection("Rebus").Get<RebusSettings>();
        return new RebusRabbitMqTransportConfiguration(routeName, rebusSettings);
    }

    public static IServiceCollection AddRebusSubscription<T>(this IServiceCollection services)
    {
        return services.AddSingleton(new SubscribeTo(typeof(T)));
    }

}

public record SubscribeTo(Type type);

public class BackgroundSubscriber : BackgroundService
{
    private readonly IBus _bus;
    private readonly IEnumerable<SubscribeTo> _subscriptions;

    private TaskCompletionSource _done = new TaskCompletionSource();
    public Task Done => _done.Task;

    public BackgroundSubscriber(IBus bus, IEnumerable<SubscribeTo> subscriptions)
    {
        _bus = bus;
        _subscriptions = subscriptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            foreach (var subscription in _subscriptions)
            {
                await _bus.Subscribe(subscription.type);
            }

            _done.SetResult();
        }
        catch (Exception e)
        {
            _done.SetException(e);
            throw;
        }

    }
}