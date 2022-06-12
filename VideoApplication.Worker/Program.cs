using VideoApplication.Worker;
using VideoApplication.Worker.ExternalPrograms;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddWorkerServices();
    })
    .Build();

await host.RunAsync();