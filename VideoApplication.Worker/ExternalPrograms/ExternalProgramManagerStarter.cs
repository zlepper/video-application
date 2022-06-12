namespace VideoApplication.Worker.ExternalPrograms;

public class ExternalProgramManagerStarter : BackgroundService
{
    private readonly ExternalProgramManager _externalProgramManager;

    public ExternalProgramManagerStarter(ExternalProgramManager externalProgramManager)
    {
        _externalProgramManager = externalProgramManager;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _externalProgramManager.Prepare(stoppingToken);
    }
}