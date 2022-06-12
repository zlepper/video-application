using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using VideoApplication.Worker.ExternalPrograms;

namespace VideoApplication.Worker.Ffmpeg;

public class FfprobeWrapper
{
    private readonly ExternalProgramRunner _externalProgramRunner;
    private readonly ILogger<FfprobeWrapper> _logger;


    public FfprobeWrapper(ExternalProgramRunner externalProgramRunner, ILogger<FfprobeWrapper> logger)
    {
        _externalProgramRunner = externalProgramRunner;
        _logger = logger;
    }

    public async Task<VideoInfo> GetVideoInformation(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Video file not found", filePath);
        }

        var args = new List<string>
        {
            "-v", "quiet",
            "-print_format", "json",
            "-show_streams",
            "-show_format",
            "-i", filePath
        };
        var probeContent = await _externalProgramRunner.Run(ExternalProgram.Ffprobe, args, cancellationToken);

        var ffProbeResult = JsonSerializer.Deserialize<FfProbeResult>(probeContent, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true, 
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        });

        if (ffProbeResult == null)
        {
            _logger.LogError("ffprobe returned null");
            throw new ArgumentNullException(nameof(ffProbeResult), "ffprobe returned null");
        }

        if (ffProbeResult.Streams == null || ffProbeResult.Streams.Count == 0)
        {
            _logger.LogError("No streams in file");
            throw new ArgumentNullException(nameof(ffProbeResult.Streams), "No streams in file");
        }


        var streams = ffProbeResult.Streams.Select(stream =>
        {
            var streamTitle = stream.Tags?.Title ?? $"{stream.CodecType}-stream-{stream.Index}";
            var streamType = Enum.Parse<StreamType>(stream.CodecType, true);
            return new StreamInfo(streamTitle, streamType, stream.CodecName);
        }).ToList();

        var duration = ffProbeResult.Format?.Duration ?? ffProbeResult.Streams.Max(s => s.Duration);
        
        return new VideoInfo(streams, TimeSpan.FromSeconds(duration));
    }


    private record FfProbeResult(List<Stream>? Streams, Format? Format);

    private record Format
    {
        public double Duration { get; set; }
    }

    private record Stream
    {
        public int Index { get; set; }
        [JsonPropertyName("codec_name")] public string CodecName { get; set; } = null!;
        public string Profile { get; set; } = null!;
        [JsonPropertyName("codec_type")] public string CodecType { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        [JsonPropertyName("r_frame_rate")] public string RFrameRate { get; set; } = null!;
        [JsonPropertyName("avg_frame_rate")] public string AvgFrameRate { get; set; } = null!;
        
        [JsonPropertyName("duration")]
        public double Duration { get; set; } = 0;
        public Tags? Tags { get; set; }
    }

    private record Tags
    {
        public string? Title { get; set; }
    }
}

public class DoubleJsonConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDouble();
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString()!;
            if (double.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
            {
                return d;
            }
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

public record VideoInfo(List<StreamInfo> Streams, TimeSpan Duration);

public record StreamInfo(string Title, StreamType StreamType, string CodecName);

public enum StreamType
{
    Video,
    Audio
}