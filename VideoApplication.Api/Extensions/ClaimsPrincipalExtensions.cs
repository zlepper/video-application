using System.Security.Claims;

namespace VideoApplication.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetId(this ClaimsPrincipal principal)
    {
        return GetClaimValue(principal, ClaimTypes.NameIdentifier);
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
    
    public const string AccessKeyClaimType = "accessKey";
}