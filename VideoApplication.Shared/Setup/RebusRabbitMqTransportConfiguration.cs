using Rebus.Config;
using Rebus.Transport;

namespace VideoApplication.Shared.Setup;

internal class RebusRabbitMqTransportConfiguration : IRebusTransportConfiguration
{
    private readonly RebusSettings _rebusSettings;

    internal RebusRabbitMqTransportConfiguration(RouteName routeName, RebusSettings rebusSettings)
    {
        _rebusSettings = rebusSettings;
        RouteName = routeName;
    }

    public RouteName RouteName { get; }
    public void ConfigureTransport(StandardConfigurer<ITransport> t)
    {
        t.UseRabbitMq($"amqp://{_rebusSettings.Host}", RouteName)
            .CustomizeConnectionFactory(f =>
            {
                f.UserName = _rebusSettings.Username;
                f.Password = _rebusSettings.Password;
                f.VirtualHost = _rebusSettings.VHost;

                return f;
            });
    }
}