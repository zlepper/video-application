namespace VideoApplication.Worker.Shared.Events;

public record QueuedVideoTranscoding(int Height, int FrameRate) : QueuedTranscoding;