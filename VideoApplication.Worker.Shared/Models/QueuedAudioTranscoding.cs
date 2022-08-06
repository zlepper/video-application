namespace VideoApplication.Worker.Shared.Events;

public record QueuedAudioTranscoding(string Name, int StreamIndex) : QueuedTranscoding;