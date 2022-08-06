namespace VideoApplication.Worker.Ffmpeg;

public record StreamInfo(string Title, StreamType StreamType, string CodecName, int Width, int Height, int StreamIndex,
    int FrameRate);