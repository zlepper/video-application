namespace VideoApplication.Worker.Ffmpeg;

public class FfmpegCommandBuilder
{
    private readonly List<string> _args = new()
    {
        "-hide_banner",
        "-nostats",
        "-nostdin",
        "-progress", "-",
        "-threads", "30"
        // "-loglevel", "quiet",
    };

    public void WithInput(string path)
    {
        _args.Add("-i");
        _args.Add(path);
    }

    public void WithVideoOnlyOutput(int videoHeight, string path)
    {
        _args.Add("-map");
        _args.Add("0:v");
        _args.Add("-vf");
        _args.Add($"scale={videoHeight}:-2");
        _args.Add(path);
    }
    
    public void WithVideoFullOutput(int videoHeight, string path)
    {
        _args.Add("-vf");
        _args.Add($"scale={videoHeight}:-2");
        _args.Add(path);
    }
    
    public void WithAudioOutput(int inputStreamNumber, string path)
    {
        _args.Add("-map");
        _args.Add($"0:{inputStreamNumber}");
        _args.Add(path);
    }
    
    public IReadOnlyList<string> Build()
    {
        return _args;
    }
}