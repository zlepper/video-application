using System.Runtime.CompilerServices;
using System.Text;
using Rebus.Bus;
using Rebus.Transport;
using VideoApplication.Worker.ExternalPrograms;
using VideoApplication.Worker.Models;
using VideoApplication.Worker.Shared.Events;

namespace VideoApplication.Worker.Ffmpeg;

public class FfmpegConverter
{
    private readonly ExternalProgramRunner _externalProgramRunner;
    private readonly ILogger<FfmpegConverter> _logger;
    private readonly FfprobeWrapper _ffprobeWrapper;
    private readonly TempFileProvider _tempFileProvider;
    private readonly IBus _bus;

    public FfmpegConverter(ExternalProgramRunner externalProgramRunner, ILogger<FfmpegConverter> logger,
        FfprobeWrapper ffprobeWrapper, TempFileProvider tempFileProvider, IBus bus)
    {
        _externalProgramRunner = externalProgramRunner;
        _logger = logger;
        _ffprobeWrapper = ffprobeWrapper;
        _tempFileProvider = tempFileProvider;
        _bus = bus;
    }

    private static readonly IReadOnlyDictionary<int, int> AudioBitRatesForResolution = new Dictionary<int, int>
    {
        {480, 48},
        {720, 64},
        {1080, 96},
        {1440, 128},
        {2160, 192},
        {4320, 192},
    };

    public async Task<string> Convert(Guid videoId, string inputFile, List<QueuedTranscoding> queuedTranscodings,
        CancellationToken cancellationToken)
    {
        var outputDirectory = _tempFileProvider.GetTempDirectory();
        
        var arguments = new List<string>
        {
            "-hide_banner",
            "-nostats",
            "-nostdin",
            "-progress", "-",
            "-i", inputFile
        };

        var videoTranscodings = queuedTranscodings.OfType<QueuedVideoTranscoding>().OrderByDescending(v => v.Height)
            .ToList();
        var audioTranscodings = queuedTranscodings.OfType<QueuedAudioTranscoding>().ToList();

        arguments.Add("-filter_complex");
        arguments.Add(BuildFilterCommand(videoTranscodings));


        for (var i = 0; i < videoTranscodings.Count; i++)
        {
            var transcoding = videoTranscodings[i];
            var bitRate = GetBitRate(transcoding.Height) / 1000 + "k";
            arguments.Add("-map");
            arguments.Add($"[v{transcoding.Height}pOut]");
            arguments.Add("-c:v");
            arguments.Add("libx264");
            arguments.Add("-x264-params");
            arguments.Add("nal-hrd=cbr:force-cfr=1");
            arguments.Add($"-b:v:{i}");
            arguments.Add(bitRate);
            arguments.Add($"-maxrate:v:{i}");
            arguments.Add(bitRate);
            arguments.Add($"-minrate:v:{i}");
            arguments.Add(bitRate);
            arguments.Add($"-bufsize:v:{i}");
            arguments.Add(bitRate);
            arguments.Add("-preset");
            arguments.Add("slow");
            // arguments.Add("-g"); // Not sure what these does exactly...
            // arguments.Add("48");
            arguments.Add("-sc_threshold");
            arguments.Add("0");
            arguments.Add("-keyint_min");
            arguments.Add((transcoding.FrameRate * SecondsPerFileSplit).ToString());
        }

        var audioIndex = 0;
        foreach (var videoTranscoding in videoTranscodings)
        {
            foreach (var audioTranscoding in audioTranscodings)
            {
                var bitRate = AudioBitRatesForResolution[videoTranscoding.Height];
                arguments.Add("-map");
                arguments.Add($"a:{audioTranscoding.StreamIndex}");
                arguments.Add($"-c:a:{audioIndex}");
                arguments.Add("aac");
                arguments.Add($"-b:a:{audioIndex}");
                arguments.Add($"{bitRate}k");
                arguments.Add("-ac");
                arguments.Add("2");
                audioIndex++;
            }
        }

        arguments.Add("-f");
        arguments.Add("hls");
        arguments.Add("-hls_time");
        arguments.Add(SecondsPerFileSplit.ToString());
        arguments.Add("-hls_playlist_type");
        arguments.Add("vod");
        arguments.Add("-hls_flags");
        arguments.Add("independent_segments");
        arguments.Add("-hls_segment_type");
        arguments.Add("mpegts");
        arguments.Add("-hls_segment_filename");
        arguments.Add(Path.Join(outputDirectory, "stream_%v/data%04d.ts"));
        arguments.Add("-master_pl_name");
        // I'm not sure if this is intended, or a bug in FFMPEG, however if we specify the full path to the master file, then the path gets repeated twice, causing ffmpeg to fail to create
        // the master file, but doesn't actual fail the command itself (So 2 problems, really)
        arguments.Add("master.m3u8"); 
        arguments.Add("-var_stream_map");
        arguments.Add(BuildStreamMap(videoTranscodings, audioTranscodings));
        arguments.Add(Path.Join(outputDirectory, "stream_%v/info.m3u8"));
        
        
        await _externalProgramRunner.RunWithOutputHandler(ExternalProgram.Ffmpeg, arguments,
            CreateOutputHandler(videoId, cancellationToken), cancellationToken);

        return outputDirectory;
    }

    private const int SecondsPerFileSplit = 10;

    private string BuildFilterCommand(List<QueuedVideoTranscoding> videoParts)
    {
        var sb = new StringBuilder();

        // [0]split=4[v1440p][v1080p][v720p][v480p]; 
        sb.Append("[0]")
            .Append("split=")
            .Append(videoParts.Count);

        foreach (var part in videoParts)
        {
            sb.Append("[v")
                .Append(part.Height)
                .Append("p]");
        }

        sb.Append("; ");

        // [v1440p]scale=w=2560:h=1440:force_original_aspect_ratio=decrease,pad=2560:1440:(ow-iw)/2:(oh-ih)/2[v1440pOut]; 
        for (var index = 0; index < videoParts.Count; index++)
        {
            var part = videoParts[index];
            var width = Math.Ceiling(part.Height * 16 / 9d);
            sb.Append("[v")
                .Append(part.Height)
                .Append("p]scale=w=")
                .Append(width)
                .Append(":h=")
                .Append(part.Height)
                .Append(":force_original_aspect_ratio=decrease,pad=")
                .Append(width)
                .Append(':')
                .Append(part.Height)
                .Append(":(ow-iw)/2:(oh-ih)/2[v")
                .Append(part.Height)
                .Append("pOut]");

            if (index < videoParts.Count - 1)
            {
                sb.Append("; ");
            }
        }

        return sb.ToString();
    }

    private string BuildStreamMap(List<QueuedVideoTranscoding> videoTranscodings,
        List<QueuedAudioTranscoding> audioTranscodings)
    {
        // v:0,a:0,a:4 v:1,a:1,a:5 v:2,a:2,1:6 v:3,a:3,a:7
        var sb = new StringBuilder();
        var audioIndex = 0;
        for (var videoIndex = 0; videoIndex < videoTranscodings.Count; videoIndex++)
        {
            var videoTranscoding = videoTranscodings[videoIndex];
            sb.Append("v:")
                .Append(videoIndex);
            foreach (var _ in audioTranscodings)
            {
                sb.Append(",a:")
                    .Append(audioIndex);

                audioIndex++;
            }

            sb
                .Append(",name:")
                .Append(videoTranscoding.Height)
                .Append('p');

            if (videoTranscoding.FrameRate > 30)
            {
                sb.Append(videoTranscoding.FrameRate)
                    .Append("fps");
            }
            
            sb.Append(' ');
        }

        return sb.ToString();
    }

    private static int GetBitRate(int height)
    {
        // height * (16 / 9) gives width
        var width = height * 16 / 9;
        // multiply height and width to get total number of pixels
        var pixelCount = width * height;
        // allows 0.3 bit per pixel
        return (int) (pixelCount * 0.30d);
    }

    private async IAsyncEnumerable<FfmpegProgress> ParseProgressOutput(TextReader stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var time = TimeSpan.Zero;

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await stream.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
            {
                yield break;
            }

            var parts = line.Split('=', 2);
            var key = parts[0];
            var value = parts[1];

            switch (key)
            {
                case "out_time":
                    time = TimeSpan.Parse(value);
                    break;
                case "progress":
                {
                    yield return new FfmpegProgress(time);
                    if (value == "end")
                    {
                        yield break;
                    }

                    break;
                }
            }
        }
    }

    private Func<StreamReader, Task> CreateOutputHandler(Guid videoId, CancellationToken cancellationToken)
    {
        return async stream =>
        {
            await foreach (var progressEvent in ParseProgressOutput(stream, cancellationToken))
            {
                using (new RebusTransactionScopeSuppressor())
                {
                    _logger.LogInformation("ffmpeg progress: {@Progress}", progressEvent);
                    await _bus.Publish(new VideoTranscodeProgress(videoId, progressEvent.Time));
                }
            }
        };
    }
}

public interface IIdentifiedOutput
{
}

public record IdentifiedAudio(string Name, int StreamNumber) : IIdentifiedOutput;

public record IdentifiedVideo(int Height) : IIdentifiedOutput;