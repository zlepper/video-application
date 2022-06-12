using System.Diagnostics;

namespace VideoApplication.Worker.ExternalPrograms;

public class ExternalProgramRunner
{
    private readonly ExternalProgramManager _externalProgramManager;
    private readonly ILogger<ExternalProgramRunner> _logger;

    public ExternalProgramRunner(ExternalProgramManager externalProgramManager, ILogger<ExternalProgramRunner> logger)
    {
        _externalProgramManager = externalProgramManager;
        _logger = logger;
    }

    public async Task<string> Run(ExternalProgram program, List<string> arguments, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running program {ProgramName} with arguments [{Arguments}]", program.ProgramName, string.Join(" ", arguments));
        using var process = await PrepareProcess(program, arguments);

        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        
        process.Start();
        var errorTask = process.StandardError.ReadToEndAsync();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await errorTask;
            var output = await outputTask;
            _logger.LogError("ffprobe failed with exit code {ExitCode} and error:\n{Error}\n Output:\n{Output}", process.ExitCode, error, output);
            throw new Exception("ffprobe failed, check log files");
        }

        return await outputTask;
    }

    private async Task<Process> PrepareProcess(ExternalProgram program, List<string> arguments)
    {
        var programPath = await _externalProgramManager.GetProgramPath(program);
        var process = new Process();

        process.StartInfo.FileName = programPath;
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        return process;
    }
}