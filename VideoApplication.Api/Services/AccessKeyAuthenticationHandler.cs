using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Extensions;

namespace VideoApplication.Api.Services;


public class AccessKeyAuthenticationHandler : CookieAuthenticationHandler
{
    private readonly AccessKeyAuthenticationHelper _helper;

    public AccessKeyAuthenticationHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, AccessKeyAuthenticationHelper helper) : base(options, logger, encoder, clock)
    {
        _helper = helper;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var result = await _helper.HandleAuthenticateAsync(Scheme.Name, Request);
        if (result.None)
        {
            return await base.HandleAuthenticateAsync();
        }

        return result;
    }
}

public class AccessKeyAuthenticationHelper
{
    private readonly ILogger<AccessKeyAuthenticationHandler> _logger;
    private readonly VideoApplicationDbContext _context;
    private readonly ICache<Guid> _accessKeyCache;
    private readonly UserManager<User> _userManager;
    private readonly IUserClaimsPrincipalFactory<User> _claimsPrincipalFactory;
    private readonly ICache<CachedClaimsPrincipal> _principalCache;

    public AccessKeyAuthenticationHelper(ILogger<AccessKeyAuthenticationHandler> logger, VideoApplicationDbContext context, ICache<Guid> accessKeyCache, UserManager<User> userManager, IUserClaimsPrincipalFactory<User> claimsPrincipalFactory, ICache<CachedClaimsPrincipal> principalCache)
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
        var token = GetAccessKeyToken(request);

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogDebug("No access key token was provided");
            return AuthenticateResult.NoResult();
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var hashString = Convert.ToHexString(hash);


        var userId = await _accessKeyCache.Get(hashString, TimeSpan.FromMinutes(5), async () =>
        {
            return await _context.AccessKeys.Where(ak => ak.Value == hashString)
                    .Select(ak => ak.UserId)
                    .FirstOrDefaultAsync();
        }, request.HttpContext.RequestAborted);
        
        if (userId == default)
        {
            _logger.LogInformation("Access key was not found: {hashString}", hashString);
            return AuthenticateResult.Fail("AccessKey not found");
        }

        var principal = await _principalCache.Get(userId.ToString(), TimeSpan.FromMinutes(5), async () =>
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return await _claimsPrincipalFactory.CreateAsync(user);
        });

        principal.Claims.Add(new CachedClaim("", new Dictionary<string, string>(), ClaimsPrincipalExtensions.AccessKeyClaimType, hashString, "", "string"));
        
        var ticket = new AuthenticationTicket(principal, schemeName);

        return AuthenticateResult.Success(ticket);
    }

    private static string? GetAccessKeyToken(HttpRequest request)
    {
        var authenticationHeaderValue = request.GetTypedHeaders().Get<AuthenticationHeaderValue>(HeaderNames.Authorization);

        if (authenticationHeaderValue == null)
        {
            return null;
        }

        if (authenticationHeaderValue.Scheme.Equals("bearer", StringComparison.OrdinalIgnoreCase))
        {
            return authenticationHeaderValue.Parameter;
        }

        return null;
    }
    
    
    public record CachedClaimsPrincipal(List<CachedClaim> Claims)
    {
        private ClaimsPrincipal ToPrincipal()
        {
            var identity = new ClaimsIdentity("AccessKey");
            identity.AddClaims(Claims.Select(c => c.ToClaim()));
            return new ClaimsPrincipal(identity);
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

}