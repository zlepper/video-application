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

    public async Task<string> Run(ExternalProgram program, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
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
            _logger.LogError("{programName} failed with exit code {ExitCode} and error:\n{Error}\n Output:\n{Output}", program.ProgramName, process.ExitCode, error, output);
            throw new Exception($"{program.ProgramName} failed, check log files: \n" + output + "\nError:\n" + error);
        }

        return await outputTask;
    }

    public async Task RunWithOutputHandler(ExternalProgram program, IReadOnlyList<string> arguments,
        Func<StreamReader, Task> processOutput, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running program {ProgramName} with arguments [{Arguments}]", program.ProgramName, string.Join(" ", arguments));
        using var process = await PrepareProcess(program, arguments);

        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        
        process.Start();
        process.PriorityClass = ProcessPriorityClass.BelowNormal;
        await using var reg = cancellationToken.Register(() =>
        {
            // ReSharper disable AccessToDisposedClosure
            if (!process.HasExited)
            {
                process.Kill(true);
            }
            // ReSharper restore AccessToDisposedClosure
        });
        var errorTask = process.StandardError.ReadToEndAsync();
        var outputTask = processOutput(process.StandardOutput);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var error = await errorTask;
            _logger.LogError("{programName} failed with exit code {ExitCode} and error:\n{Error}\n", program.ProgramName, process.ExitCode, error);
            throw new Exception($"{program.ProgramName} failed, check log files: \nError:\n" + error);
        }

        await outputTask;
    }

    private async Task<Process> PrepareProcess(ExternalProgram program, IEnumerable<string> arguments)
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