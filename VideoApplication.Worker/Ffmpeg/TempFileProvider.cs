namespace VideoApplication.Worker.Ffmpeg;

public sealed class TempFileProvider : IDisposable
{
    private readonly ILogger<TempFileProvider> _logger;

    public TempFileProvider(ILogger<TempFileProvider> logger)
    {
        _logger = logger;
    }

    private readonly string _workFolderName = Path.Join(Path.GetTempPath(), "videoapplication-workarea", Guid.NewGuid().ToString("N"));

    private bool _initialized = false;
    
    public string GetTempFileName(string extensions)
    {
        Initialize();

        var randomFileName = Guid.NewGuid().ToString("N");

        return Path.Join(_workFolderName, $"{randomFileName}.{extensions}");
    }

    public string GetTempDirectory()
    {
        var randomFolderName = Guid.NewGuid().ToString("N");

        return Path.Join(_workFolderName, randomFolderName);
    }

    private void Initialize()
    {
        if (!_initialized)
        {
            Directory.CreateDirectory(_workFolderName);
            _initialized = true;
        }
    }

    void IDisposable.Dispose()
    {
        if (_initialized)
        {
            try
            {
                Directory.Delete(_workFolderName, true);
            }
            catch (DirectoryNotFoundException)
            {
                _logger.LogError("Directory was not found when deleting work folder");
            }

            _initialized = false;
        }
    }
}