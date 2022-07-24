using System.Security.Claims;

namespace VideoApplication.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal principal)
    {
        var id = GetClaimValue(principal, ClaimTypes.NameIdentifier);

        if (Guid.TryParse(id, out var guid))
        {
            return guid;
        }
        
        throw new ArgumentException($"User Id was not a valid Guid. Got '{id}'", nameof(id));
    }

    public static Guid? GetIdOrNull(this ClaimsPrincipal principal)
    {
        var id = GetClaimValue(principal, ClaimTypes.NameIdentifier);

        if (Guid.TryParse(id, out var guid))
        {
            return guid;
        }

        return null;
    }

    public static string GetAccessKey(this ClaimsPrincipal principal)
    {
        return GetClaimValue(principal, AccessKeyClaimType);
    }
    
    private static string GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        var claim = principal.FindFirst(claimType) ??
            throw new NullReferenceException($"Claims principal does not contain a {claimType} claim");

        return claim.Value;
    }
    
    private static string? GetClaimValueOrNull(ClaimsPrincipal principal, string claimType)
    {
        var claim = principal.FindFirst(claimType);
        return claim?.Value;
    }
    
    public const string AccessKeyClaimType = "accessKey";
}