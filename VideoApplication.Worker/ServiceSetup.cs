using VideoApplication.Worker.ExternalPrograms;
using VideoApplication.Worker.Ffmpeg;

namespace VideoApplication.Worker;

public static class ServiceSetup
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<ExternalProgramManager>()
            .AddSingleton<FfmpegInstaller>()
            .AddSingleton<ExternalProgramRunner>()
            .AddSingleton<FfprobeWrapper>();
        services.AddHostedService<ExternalProgramManagerStarter>();

        return services;
    }
    
}