using System.Security.Claims;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using Rebus.Bus;
using Rebus.TestHelpers;
using VideoApplication.Api.Controllers;
using VideoApplication.Api.Controllers.Auth.Requests;
using VideoApplication.Api.Controllers.Auth.Responses;
using VideoApplication.Api.Controllers.Channels.Requests;
using VideoApplication.Api.Controllers.Channels.Responses;
using VideoApplication.Api.Database;
using VideoApplication.Api.Extensions;
using VideoApplication.Api.Models;
using VideoApplication.Api.Services;

namespace VideoApplication.Api.Tests;

public abstract class TestBase
{
    private async Task<ServiceProvider> CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging(l => l.AddConsole());
        services.AddSingleton<FakeBus>();
        services.AddSingleton<IBus>(sp => sp.GetRequiredService<FakeBus>());
        services.AddCustomIdentity();
        services.AddApplicationServices();
        services.AddDistributedMemoryCache();

        var testBucketName = ConfigureTestStorage(services);

        ConfigureDbContext(services);
        
        AddMoreDependencies(services);

        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<VideoApplicationDbContext>().Database.EnsureCreatedAsync();

        var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        await s3.PutBucketAsync(testBucketName);
        
        return serviceProvider;
    }

    private static string ConfigureTestStorage(ServiceCollection services)
    {
        var testBucketName = "utb-" + Guid.NewGuid();
        services.PostConfigure<StorageSettings>(s =>
        {
            s.AccessKey = "minioadmin";
            s.SecretKey = "minioadmin";
            s.ServiceUrl = "http://localhost:9000";
            s.BucketName = testBucketName;
        });

        services.AddS3Storage(new ConfigurationRoot(ArraySegment<IConfigurationProvider>.Empty));

        return testBucketName;
    }

    protected FakeBus Bus => ServiceProvider.GetRequiredService<FakeBus>();
    
    protected virtual void AddMoreDependencies(IServiceCollection services) {}

    private ServiceProvider? _serviceProvider;
    private IServiceScope? _serviceScope;
    
    protected IServiceProvider RootServiceProvider => _serviceProvider ?? throw new Exception("ServiceProvider is not initialized");
    protected IServiceProvider ServiceProvider => _serviceScope?.ServiceProvider ?? throw new Exception("ServiceProvider is not initialized");

    protected VideoApplicationDbContext DbContext => ServiceProvider.GetRequiredService<VideoApplicationDbContext>();
    
    
    [SetUp]
    public async Task Setup()
    {
        _serviceProvider = await CreateServiceProvider();
        _serviceScope = _serviceProvider.CreateScope();
    }

    private void ConfigureDbContext(IServiceCollection services)
    {
        var databaseName = $"TestDb{Guid.NewGuid():N}";
        var connectionString = $"Host=localhost;Port=26257;Database={databaseName};Username=root;Password=root;";

        services.AddVideoApplicationDbContext(connectionString, true);
        services.AddSingleton(new TestDatabaseName(databaseName, connectionString));
    }

    private record TestDatabaseName(string DatabaseName, string ConnectionString);

    [TearDown]
    public async Task Teardown()
    {
        if (_serviceScope != null)
        {
            var name = _serviceScope.ServiceProvider.GetRequiredService<TestDatabaseName>();
            await using (var conn = new NpgsqlConnection(name.ConnectionString))
            {
                conn.Open();
                await using var command = conn.CreateCommand();
                command.CommandText = $@"DROP DATABASE ""{name.DatabaseName}"" CASCADE;";
                command.ExecuteNonQuery();
            }


            _serviceScope.Dispose();
            _serviceScope = null;
        }

        if (_serviceProvider != null)
        {
            
            var storageSettings = _serviceProvider.GetRequiredService<IOptions<StorageSettings>>();
            var s3 = _serviceProvider.GetRequiredService<IAmazonS3>();
            var objects = await s3.ListObjectsV2Async(new ListObjectsV2Request()
            {
                BucketName = storageSettings.Value.BucketName,
            });
            if (objects.S3Objects.Count > 0)
            {
                await s3.DeleteObjectsAsync(new DeleteObjectsRequest()
                {
                    Objects = objects.S3Objects.Select(o => new KeyVersion()
                    {
                        Key = o.Key,
                    }).ToList(),
                    BucketName = storageSettings.Value.BucketName
                });
            }

            await s3.DeleteBucketAsync(storageSettings.Value.BucketName);
            await _serviceProvider.DisposeAsync();
            _serviceProvider = null;
        }
    }
}

public abstract class TestBase<T> : TestBase where T : class
{
    protected override void AddMoreDependencies(IServiceCollection services)
    {
        services.TryAddScoped<T>();
        services.TryAddScoped<AuthController>();
        services.TryAddScoped<ChannelController>();
        base.AddMoreDependencies(services);
    }

    protected virtual bool ClearContextBeforeInvokingService => true;
    
    protected T Service
    {
        get
        {
            if (ClearContextBeforeInvokingService)
            {
                DbContext.ChangeTracker.Clear();
                Bus.Clear();
            }
            
            return ServiceProvider.GetRequiredService<T>();
        }
    }

    protected async Task<UserInfo> CreateTestUser()
    {
        return await ServiceProvider.GetRequiredService<AuthController>().Signup(new SignupRequest()
        {
            Email = "test@example.com",
            Name = "TestUser",
            Password = "SuperDuperSafe"
        });
    }

    protected void SetUserContext(UserInfo user)
    {
        if (Service is not ControllerBase controller)
        {
            throw new Exception($"Service is not a Controller. Either make {typeof(T)} extend {typeof(ControllerBase)} or pass user context information manually.");
        }
        
        var httpContext = new DefaultHttpContext()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new []
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            }))
        };

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    protected async Task CreateAndUseTestUser()
    {
        var testUser = await CreateTestUser();
        
        SetUserContext(testUser);
    }

    protected async Task<ChannelResponse> CreateTestChannel()
    {
        var request = new CreateChannelRequest("myChannel", "My Channel", "This is my channel for testing");

        var channelController = ServiceProvider.GetRequiredService<ChannelController>();
        channelController.ControllerContext = (Service as ControllerBase)!.ControllerContext;
        return await channelController.CreateChannel(request);
    }

    protected async Task<ChannelResponse> PrepareTestSystem()
    {
        await CreateAndUseTestUser();

        return await CreateTestChannel();
    }
}
