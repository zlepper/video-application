namespace VideoApplication.Api.Controllers.Auth.Responses;

public record UserInfo(string AccessKey, string? Name, bool IsValidated, Guid UserId);
