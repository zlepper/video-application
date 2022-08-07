using VideoApplication.Shared.Setup;
using VideoApplication.Shared.Storage;
using VideoApplication.Worker;

await Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(h =>
    {
        h.AddJsonFile("appsettings.json", false, true);
        h.AddJsonFile("appsettings.Development.json", true, true);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddWorkerServices();
        services.ConfigureRebus(ctx.Configuration.UseRebusRabbitMqTransport(RouteName.Worker));
        services.AddS3Storage(ctx.Configuration);
    })
    .Build()
    .RunAsync();
