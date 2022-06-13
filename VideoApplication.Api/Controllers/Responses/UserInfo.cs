namespace VideoApplication.Api.Controllers.Responses;

public record UserInfo(string AccessKey, string? Name, bool IsValidated, Guid UserId);
