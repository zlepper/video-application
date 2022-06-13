using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Services;

namespace VideoApplication.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock, DefaultClock>();
        services.AddSingleton(typeof(ICache<>), typeof(DistributedCache<>));
        services.AddScoped<AccessKeyAuthenticationHelper>();
        
        return services;
    }

    public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<VideoApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options => { options.User.RequireUniqueEmail = true; });


        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = null;
            options.LogoutPath = null;
            options.AccessDeniedPath = null;
        });

        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, AccessKeyAuthenticationHandler>("AccessKey", _ => { });

        return services;
    }
}