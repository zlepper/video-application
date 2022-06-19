namespace VideoApplication.Api.Shared.Events;

public record ChannelCreated(Guid Id, string DisplayName, string IdentifierName, Guid OwnerId);
