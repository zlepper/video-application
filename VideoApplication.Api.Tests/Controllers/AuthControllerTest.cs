using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using VideoApplication.Api.Controllers;
using VideoApplication.Api.Exceptions;
using VideoApplication.Api.Services;

namespace VideoApplication.Api.Tests.Controllers;

[TestFixture]
public class AuthControllerTest : TestBase<AuthController>
{
    [Test]
    public async Task CanSignup_AndLogin()
    {
        var email = "foo@example.com";
        var password = "TopSecret!1";
        var response = await Service.Signup(new SignupRequest()
        {
            Email = email,
            Name = "foo bar",
            Password = password
        });
        
        Assert.That(response.AccessKey, Is.Not.Null.And.Not.Empty);

        var loginResponse = await Service.Login(new LoginRequest()
        {
            Email = email,
            Password = password
        });
        
        Assert.That(loginResponse.AccessKey, Is.Not.Null.And.Not.Empty);

        var httpContext = new DefaultHttpContext()
        {
            Request =
            {
                Headers =
                {
                    {
                        HeaderNames.Authorization,
                        new AuthenticationHeaderValue("AccessKey", loginResponse.AccessKey).ToString()
                    }
                }
            }
        };
        var result = await ServiceProvider.GetRequiredService<AccessKeyAuthenticationHelper>()
            .HandleAuthenticateAsync("AccessKey", httpContext.Request);
        
        Assert.That(result.Succeeded);
        Assert.That(result.Principal, Is.Not.Null);
    }

    [Test]
    public async Task HandlesLoginFailure_WrongEmail()
    {
        await Service.Signup(new SignupRequest()
        {
            Email = "foo@example.com",
            Name = "foo bar",
            Password = "TopSecret!1"
        });

        Assert.That(async () =>
        {
            await Service.Login(new LoginRequest()
            {
                Email = "fooo@example.com",
                Password = "TopSecret!1"
            });
        }, Throws.TypeOf<InvalidEmailOrPasswordException>());
    }
    
    [Test]
    public async Task HandlesLoginFailure_WrongPassword()
    {
        await Service.Signup(new SignupRequest()
        {
            Email = "foo@example.com",
            Name = "foo bar",
            Password = "TopSecret!1"
        });

        Assert.That(async () =>
        {
            await Service.Login(new LoginRequest()
            {
                Email = "foo@example.com",
                Password = "TopSecet!1"
            });
        }, Throws.TypeOf<InvalidEmailOrPasswordException>());
    }
    
    [Test]
    public async Task GiveErrorIf2UsersSignUpWithSameEmail()
    {
        await Service.Signup(new SignupRequest()
        {
            Email = "foo@example.com",
            Name = "foo bar",
            Password = "TopSecret!1"
        });
        
        Assert.That(async () => {
        await Service.Signup(new SignupRequest()
        {
            Email = "foo@example.com",
            Name = "foo bar",
            Password = "TopSecret!1"
        });
        }, Throws.TypeOf<EmailAlreadyInUseException>());
        
    }
}