using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Minio;
using NodaTime;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Models;
using VideoApplication.Api.Services;
using SystemClock = NodaTime.SystemClock;

namespace VideoApplication.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton(typeof(ICache<>), typeof(DistributedCache<>));

        services.AddHttpClient();
        
        return services;
    }

    private const string AccessKeySchemeName = "AccessKey";
    
    public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, Role>(i =>
            {
                i.User.RequireUniqueEmail = true;
                i.Password.RequireDigit = false;
                i.Password.RequiredUniqueChars = 1;
                i.Password.RequireNonAlphanumeric = false;
                i.Password.RequireUppercase = false;
                i.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<VideoApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = null;
            options.LogoutPath = null;
            options.AccessDeniedPath = null;

            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
        });

        services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = AccessKeySchemeName;
            })
            .AddScheme<CookieAuthenticationOptions, AccessKeyAuthenticationHandler>(AccessKeySchemeName, _ => { });

        services.AddScoped<AccessKeyAuthenticationHelper>();

        return services;
    }

    public static IServiceCollection AddVideoApplicationDbContext(this IServiceCollection services, string connectionString, bool isDevelopment)
    {
        services.AddDbContext<VideoApplicationDbContext>(o =>
        {
            o.UseNpgsql(connectionString, b =>
            {
                b.EnableRetryOnFailure();
                b.UseNodaTime();
            });

            if (isDevelopment)
            {
                o.EnableDetailedErrors();
                o.EnableSensitiveDataLogging();
                o.ConfigureWarnings(warningActions =>
                {
                    warningActions.Throw(CoreEventId.FirstWithoutOrderByAndFilterWarning,
                        CoreEventId.RowLimitingOperationWithoutOrderByWarning, CoreEventId.DistinctAfterOrderByWithoutRowLimitingOperatorWarning);
                });
            }
        });
        return services;
    }

    public static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configurationRoot)
    {
        services.Configure<StorageSettings>(configurationRoot.GetSection("Storage"));

        services.AddSingleton(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("minio");
            
            var storageConfig = sp.GetRequiredService<IOptions<StorageSettings>>();

            return new MinioClient()
                .WithHttpClient(httpClient)
                .WithCredentials(storageConfig.Value.AccessKey, storageConfig.Value.SecretKey)
                .WithEndpoint(storageConfig.Value.ServiceUrl)
                .Build();
        });

        services.AddScoped<StorageWrapper>();


        return services;
    }
}