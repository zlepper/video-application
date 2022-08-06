namespace VideoApplication.Worker.Shared.Events;

public record VideoTranscodingsIdentified(Guid ChannelId, Guid VideoId, string OriginalFileExtension, List<QueuedTranscoding> Transcodings);