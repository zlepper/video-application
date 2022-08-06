namespace VideoApplication.Api.Shared.Events;

public record VideoUploadFinished(Guid ChannelId, Guid VideoId, string OriginalFileExtension);