namespace VideoApplication.Api.Shared.Events;

public record UserCreated(Guid Id, string Name, string Email);
