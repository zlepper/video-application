namespace VideoApplication.Worker.Shared.Events;

public record VideoTranscodeProgress(Guid VideoId, TimeSpan Time);