using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.TestHelpers;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Extensions;

namespace VideoApplication.Api.Tests;

public abstract class TestBase
{
    private SqliteConnection? _dbConnection;

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
    
    protected virtual void AddMoreDependencies(IServiceCollection services) {}

    private ServiceProvider? _serviceProvider;
    private IServiceScope? _serviceScope;
    
    protected IServiceProvider RootServiceProvider => _serviceProvider ?? throw new Exception("ServiceProvider is not initialized");
    protected IServiceProvider ServiceProvider => _serviceScope?.ServiceProvider ?? throw new Exception("ServiceProvider is not initialized");
    
    
    [SetUp]
    public void Setup()
    {
        _serviceProvider = CreateServiceProvider();
        _serviceScope = _serviceProvider.CreateScope();
    }

    private void ConfigureDbContext<T>(IServiceCollection services)
        where T : DbContext
    {
        var databaseName = Guid.NewGuid().ToString();
        var connectionString = $"DataSource={databaseName};Mode=Memory;Cache=Shared";
        _dbConnection = new SqliteConnection(connectionString);
        _dbConnection.Open();

        services.AddDbContext<T>(options =>
        {
            options.UseSqlite(connectionString)
                .EnableSensitiveDataLogging();
        });
    }

    [TearDown]
    public void Teardown()
    {
        if (_dbConnection != null)
        {
            _dbConnection.Dispose();
            _dbConnection = null;
        }

        if (_serviceScope != null)
        {
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
        base.AddMoreDependencies(services);
    }

    protected T Service => ServiceProvider.GetRequiredService<T>();
}
