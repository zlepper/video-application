using VideoApplication.Shared.Setup;
using VideoApplication.Shared.Storage;
using VideoApplication.Worker;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddWorkerServices();
        services.ConfigureRebus(ctx.Configuration.UseRebusRabbitMqTransport(RouteName.Worker));
        services.AddS3Storage(ctx.Configuration);
    })
    .Build()
    .RunAsync();
