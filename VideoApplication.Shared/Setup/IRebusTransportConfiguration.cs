using Rebus.Config;
using Rebus.Transport;

namespace VideoApplication.Shared.Setup;

public interface IRebusTransportConfiguration
{
    public RouteName RouteName { get; }

    public void ConfigureTransport(StandardConfigurer<ITransport> t);
}