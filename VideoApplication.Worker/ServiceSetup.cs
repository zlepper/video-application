using Rebus.Config;
using VideoApplication.Api.Shared.Events;
using VideoApplication.Shared.Setup;
using VideoApplication.Worker.ExternalPrograms;
using VideoApplication.Worker.Ffmpeg;
using VideoApplication.Worker.Handlers;
using VideoApplication.Worker.Shared.Events;

namespace VideoApplication.Worker;

public static class ServiceSetup
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services
            .AddSingleton<ExternalProgramManager>()
            .AddSingleton<FfmpegInstaller>()
            .AddSingleton<ExternalProgramRunner>()
            .AddSingleton<FfprobeWrapper>()
            .AddScoped<TempFileProvider>()
            .AddScoped<FfmpegConverter>()
            .AddHostedService<ExternalProgramManagerStarter>();

        services.AddRebusHandler<TranscodeVideoHandler>();
        services.AddRebusSubscription<VideoUploadFinished>();
        services.AddRebusSubscription<VideoTranscodingsIdentified>();
        
        return services;
    }
    
}