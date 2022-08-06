namespace VideoApplication.Worker.Shared.Events;

public record VideoTranscodingFinished(Guid ChannelId, Guid VideoId);
