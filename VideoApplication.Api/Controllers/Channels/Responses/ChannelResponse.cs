namespace VideoApplication.Api.Controllers.Channels.Responses;

public record ChannelResponse(Guid Id, string IdentifierName, string DisplayName, string Description, bool IsOwner);
