using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Rebus.Bus;
using Rebus.TestHelpers;
using VideoApplication.Api.Controllers;
using VideoApplication.Api.Controllers.Auth.Requests;
using VideoApplication.Api.Controllers.Auth.Responses;
using VideoApplication.Api.Database;
using VideoApplication.Api.Extensions;

namespace VideoApplication.Api.Tests;

public abstract class TestBase
{
    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging(l => l.AddConsole());
        services.AddSingleton<FakeBus>();
        services.AddSingleton<IBus>(sp => sp.GetRequiredService<FakeBus>());
        services.AddCustomIdentity();
        services.AddApplicationServices();
        services.AddDistributedMemoryCache();
        
        ConfigureDbContext<VideoApplicationDbContext>(services);
        
        AddMoreDependencies(services);

        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<VideoApplicationDbContext>().Database.EnsureCreated();

        return serviceProvider;
    }

    protected FakeBus Bus => ServiceProvider.GetRequiredService<FakeBus>();
    
    protected virtual void AddMoreDependencies(IServiceCollection services) {}

    private ServiceProvider? _serviceProvider;
    private IServiceScope? _serviceScope;
    
    protected IServiceProvider RootServiceProvider => _serviceProvider ?? throw new Exception("ServiceProvider is not initialized");
    protected IServiceProvider ServiceProvider => _serviceScope?.ServiceProvider ?? throw new Exception("ServiceProvider is not initialized");

    protected VideoApplicationDbContext DbContext => ServiceProvider.GetRequiredService<VideoApplicationDbContext>();
    
    
    [SetUp]
    public void Setup()
    {
        _serviceProvider = CreateServiceProvider();
        _serviceScope = _serviceProvider.CreateScope();
    }

    private void ConfigureDbContext<T>(IServiceCollection services)
        where T : DbContext
    {
        var databaseName = $"TestDb{Guid.NewGuid():N}";
        var connectionString = $"Host=localhost;Port=26257;Database={databaseName};Username=root;Password=root;";

        services.AddVideoApplicationDbContext(connectionString, (db, _) => db.EnableSensitiveDataLogging());
        services.AddSingleton(new TestDatabaseName(databaseName, connectionString));
    }

    private record TestDatabaseName(string DatabaseName, string ConnectionString);

    [TearDown]
    public void Teardown()
    {
        if (_serviceScope != null)
        {
            var name = _serviceScope.ServiceProvider.GetRequiredService<TestDatabaseName>();
            using (var conn = new NpgsqlConnection(name.ConnectionString))
            {
                conn.Open();
                using var command = conn.CreateCommand();
                command.CommandText = $@"DROP DATABASE ""{name.DatabaseName}"" CASCADE;";
                command.ExecuteNonQuery();
            }
            
            _serviceScope.Dispose();
            _serviceScope = null;
        }

        if (_serviceProvider != null)
        {
            _serviceProvider.Dispose();
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
}
