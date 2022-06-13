using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;

namespace VideoApplication.Api.Services;


public class AccessKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AccessKeyAuthenticationHelper _helper;

    public AccessKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, AccessKeyAuthenticationHelper helper) : base(options, logger, encoder, clock)
    {
        _helper = helper;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return _helper.HandleAuthenticateAsync(Scheme.Name, Request);
    }
}

public record CachedClaimsPrincipal(List<CachedClaim> Claims)
{
    private ClaimsPrincipal ToPrincipal()
    {
        var claimIdentity = new ClaimsIdentity(Claims.Select(c => c.ToClaim()));
        return new ClaimsPrincipal(claimIdentity);
    }

    public static implicit operator ClaimsPrincipal(CachedClaimsPrincipal p) => p.ToPrincipal();
    public static implicit operator CachedClaimsPrincipal(ClaimsPrincipal p) => new(p.Claims.Select(CachedClaim.Create).ToList());
}

public record CachedClaim(string Issuer, Dictionary<string,string> Properties, string Type, string Value, string OriginalIssuer, string ValueType)
{
    public static CachedClaim Create(Claim claim)
    {
        return new(claim.Issuer, claim.Properties.ToDictionary(p => p.Key, p => p.Value), claim.Type, claim.Value,
            claim.OriginalIssuer, claim.ValueType);
    }

    public Claim ToClaim()
    {
        var c = new Claim(Type, Value, ValueType, Issuer, OriginalIssuer);
        foreach (var property in Properties)
        {
            c.Properties[property.Key] = property.Value;
        }

        return c;
    }
}

public record CachedAccessKey(Guid Id, string Value, Guid UserId)
{
    public static implicit operator CachedAccessKey(AccessKey ak) => new(ak.Id, ak.Value, ak.UserId);
}

public class AccessKeyAuthenticationHelper
{
    private readonly ILogger<AccessKeyAuthenticationHandler> _logger;
    private readonly VideoApplicationDbContext _context;
    private readonly ICache<CachedAccessKey?> _accessKeyCache;
    private readonly UserManager<User> _userManager;
    private readonly IUserClaimsPrincipalFactory<User> _claimsPrincipalFactory;
    private readonly ICache<CachedClaimsPrincipal> _principalCache;

    public AccessKeyAuthenticationHelper(ILogger<AccessKeyAuthenticationHandler> logger, VideoApplicationDbContext context, ICache<CachedAccessKey?> accessKeyCache, UserManager<User> userManager, IUserClaimsPrincipalFactory<User> claimsPrincipalFactory, ICache<CachedClaimsPrincipal> principalCache)
    {
        _logger = logger;
        _context = context;
        _accessKeyCache = accessKeyCache;
        _userManager = userManager;
        _claimsPrincipalFactory = claimsPrincipalFactory;
        _principalCache = principalCache;
    }

    public async Task<AuthenticateResult> HandleAuthenticateAsync(string schemeName, HttpRequest request)
    {
        var token = GetAccessKeyToken(schemeName, request);

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogDebug("No access key token was provided");
            return AuthenticateResult.NoResult();
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var hashString = Convert.ToHexString(hash);


        var accessKey = await _accessKeyCache.Get(hashString, TimeSpan.FromMinutes(5), async () =>
        {
            var a = await _context.AccessKeys.FirstOrDefaultAsync(ak => ak.Value == hashString);
            if (a == null)
            {
                return null;
            }

            return a;
        }, request.HttpContext.RequestAborted);
        if (accessKey == null)
        {
            _logger.LogInformation("Access key was not found: {hashString}", hashString);
            return AuthenticateResult.Fail("AccessKey not found");
        }

        var principal = await _principalCache.Get(accessKey.UserId.ToString(), TimeSpan.FromMinutes(5), async () =>
        {
            var user = await _userManager.FindByIdAsync(accessKey.UserId.ToString());
            var claimsPrincipal = await _claimsPrincipalFactory.CreateAsync(user);

            return claimsPrincipal;
        });

        var ticket = new AuthenticationTicket(principal, schemeName);

        return AuthenticateResult.Success(ticket);
    }

    private static string? GetAccessKeyToken(string schemeName, HttpRequest request)
    {
        var authenticationHeaderValue = request.GetTypedHeaders().Get<AuthenticationHeaderValue>(HeaderNames.Authorization);

        if (authenticationHeaderValue == null)
        {
            return null;
        }

        if (authenticationHeaderValue.Scheme == schemeName)
        {
            return authenticationHeaderValue.Parameter;
        }

        return null;
    }
}