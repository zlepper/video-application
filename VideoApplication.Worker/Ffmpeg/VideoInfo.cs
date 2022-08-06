namespace VideoApplication.Worker.Ffmpeg;

public record VideoInfo(List<StreamInfo> Streams, TimeSpan Duration);