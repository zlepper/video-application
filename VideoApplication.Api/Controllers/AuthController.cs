using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Bus;
using VideoApplication.Api.Controllers.Auth.Requests;
using VideoApplication.Api.Controllers.Auth.Responses;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Exceptions;
using VideoApplication.Api.Exceptions.Auth;
using VideoApplication.Api.Extensions;
using VideoApplication.Api.Services;
using VideoApplication.Api.Shared.Events;

namespace VideoApplication.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly VideoApplicationDbContext _context;
    private readonly IClock _clock;
    private readonly IBus _bus;

    public AuthController(ILogger<AuthController> logger, UserManager<User> userManager,
        SignInManager<User> signInManager, VideoApplicationDbContext context, IClock clock, IBus bus)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _clock = clock;
        _bus = bus;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<UserInfo> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogInformation("User with email {email} was not found", request.Email);
            throw new InvalidEmailOrPasswordException();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (result.Succeeded)
        {
            return await CreateAuthResponse(user);
        }
        else
        {
            _logger.LogInformation("Login failed. Reason: {Reason}", result);
            throw new InvalidEmailOrPasswordException();
        }
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<UserInfo> Signup(SignupRequest request)
    {
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            Name = request.Name,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            await _bus.Publish(new UserCreated(user.Id, user.Name, user.Email));
            return await CreateAuthResponse(user);
        }
        else
        {
            foreach (var identityError in result.Errors)
            {
                switch (identityError.Code)
                {
                    case nameof(IdentityErrorDescriber.DuplicateEmail):
                        throw new EmailAlreadyInUseException(user.Email);
                }
            }

            _logger.LogError("Failed to create user: {Errors}", string.Join(", ", result.Errors.Single().Code));
            throw new Exception("Failed to create user, check logs.");
        }
    }

    [HttpGet("who-am-i")]
    [Authorize]
    public async Task<UserInfo> WhoAmI()
    {
        var user = await _userManager.FindByIdAsync(User.GetId().ToString());

        var validated = await _userManager.IsEmailConfirmedAsync(user);

        return new UserInfo("<Redacted>", user.Name, validated, user.Id);
    }

    [HttpDelete("logout")]
    [Authorize]
    public async Task<object> Logout()
    {
        var akToken = User.GetAccessKey();
        var ak = await _context.AccessKeys.FirstOrDefaultAsync(a => a.Value == akToken);
        if (ak != null)
        {
            _context.AccessKeys.Remove(ak);
            await _context.SaveChangesAsync();
        }
        
        await _signInManager.SignOutAsync();

        return new object();
    }


    private async Task<UserInfo> CreateAuthResponse(User user)
    {
        var ak = await CreateAccessKey(user);
        var validated = await _userManager.IsEmailConfirmedAsync(user);
        return new UserInfo(ak, user.Name, validated, user.Id);
    }

    private async Task<string> CreateAccessKey(User user)
    {
        var tokenRawValue = Guid.NewGuid().ToString();
        var hashedValue = SHA256.HashData(Encoding.UTF8.GetBytes(tokenRawValue));
        var hashedString = Convert.ToHexString(hashedValue);
        
        var ak = new AccessKey()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LoginDate = _clock.GetCurrentInstant(),
            Value = hashedString
        };
        _context.AccessKeys.Add(ak);
        await _context.SaveChangesAsync();

        return tokenRawValue;
    }


}