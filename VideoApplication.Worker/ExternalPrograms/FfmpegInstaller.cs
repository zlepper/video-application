using System.IO.Compression;

namespace VideoApplication.Worker.ExternalPrograms;

public class FfmpegInstaller
{
    private readonly ILogger<FfmpegInstaller> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly Uri DownloadUrl =
        new Uri("https://application-dependencies-download.zlepper.workers.dev/ffmpeg-5.0.1.zip");
    private const string ffprobeName = "ffprobe.exe";
    private const string ffmpegName = "ffmpeg.exe";

    public FfmpegInstaller(ILogger<FfmpegInstaller> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task InstallFfmpeg(DirectoryInfo targetDirectory, CancellationToken cancellationToken)
    {
        if (!NeedsInstall(targetDirectory))
        {
            _logger.LogInformation("Seems ffmpeg is already installed");
            return;
        }
        
        _logger.LogInformation("Downloading ffmpeg");
        var httpClient = _httpClientFactory.CreateClient("ffmpeg installer");

        _logger.LogInformation("Starting ffmpeg download stream");
        await using var stream = await httpClient.GetStreamAsync(DownloadUrl, cancellationToken);
        
        using var zipFile = new ZipArchive(stream, ZipArchiveMode.Read);
        zipFile.ExtractToDirectory(targetDirectory.FullName, true);
        
        _logger.LogInformation("ffmpeg download complete");
    }

    private static bool NeedsInstall(DirectoryInfo targetDirectory)
    {
        return !File.Exists(Path.Join(targetDirectory.FullName, ffprobeName)) ||
               !File.Exists(Path.Join(targetDirectory.FullName, ffmpegName));
    }
}