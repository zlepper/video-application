namespace VideoApplication.Worker.Shared.Events;

public record VideoTranscodingsIdentified(Guid ChannelId, Guid VideoId, string OriginalFileExtension, TimeSpan Duration, List<QueuedTranscoding> Transcodings);