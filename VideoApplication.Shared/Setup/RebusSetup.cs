using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using VideoApplication.Api.Shared;
using VideoApplication.Worker.Shared;

namespace VideoApplication.Shared.Setup;

public static class RebusSetup
{
    public static void ConfigureRebus(this IServiceCollection serviceCollection, IConfiguration configurationRoot,
        RouteName routeName)
    {
        var rebusSettings = configurationRoot.GetSection("Rebus").Get<RebusSettings>();

        serviceCollection.AddRebus((c, sp) => c
            .Logging(l => l.MicrosoftExtensionsLogging(sp.GetRequiredService<ILoggerFactory>()))
            .Options(o =>
            {
                o.EnableDiagnosticSources();
                o.SimpleRetryStrategy($"{routeName}.error");
            })
            .Routing(r => r.TypeBased()
                .MapAssemblyOf<VideoWorkerShared>(RouteName.Worker)
                .MapAssemblyOf<VideoApiShared>(RouteName.Api))
            .Transport(t => t.UseRabbitMq($"amqp://{rebusSettings.Host}", routeName)
                .CustomizeConnectionFactory(f =>
                {
                    f.UserName = rebusSettings.Username;
                    f.Password = rebusSettings.Password;
                    f.VirtualHost = rebusSettings.VHost;

                    return f;
                }))
        );
    }
}

public class RebusSettings
{
    public string Host { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string VHost { get; set; } = null!;
}

public class RouteName
{
    public static RouteName Api = new RouteName("api");
    public static RouteName Worker = new RouteName("worker");

    internal readonly string Value;

    internal RouteName(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(RouteName r) => r.Value;
}