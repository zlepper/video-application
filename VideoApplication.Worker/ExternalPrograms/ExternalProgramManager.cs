using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;

namespace VideoApplication.Worker.ExternalPrograms;

public class ExternalProgramManager
{
    private readonly FfmpegInstaller _ffmpegInstaller;
    private readonly IOptions<WorkerSettings> _settings;

    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ExternalProgramManager(FfmpegInstaller ffmpegInstaller, IOptions<WorkerSettings> settings)
    {
        _ffmpegInstaller = ffmpegInstaller;
        _settings = settings;
    }

    public async Task Prepare(CancellationToken cancellationToken)
    {
        var directoryInfo = new DirectoryInfo(_settings.Value.ProgramDirectory);
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        await _ffmpegInstaller.InstallFfmpeg(directoryInfo, cancellationToken);
        
        _ready.SetResult();
    }

    public async Task<string> GetProgramPath(ExternalProgram program)
    {
        await _ready.Task;
        
        var basePath = Path.Join(_settings.Value.ProgramDirectory, program.ProgramName);
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return basePath + ".exe";
        }
        else
        {
            return basePath;
        }
    }
}