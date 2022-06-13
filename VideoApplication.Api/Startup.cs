using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Extensions;
using VideoApplication.Api.Middleware;
using VideoApplication.Shared.Setup;

namespace VideoApplication.Api;

public class Startup 
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSingleton<ExceptionStatusMiddleware>();
        services.AddStackExchangeRedisCache(o =>
        {
            o.Configuration = _configuration.GetConnectionString("Redis");
        });
        services.AddCustomIdentity();

        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<VideoApplicationDbContext>(o =>
        {
            o.UseNpgsql(connectionString);
        });
        
        services.ConfigureRebus(_configuration, RouteName.Api);
        
    }

    public void Configure(IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        // Configure the HTTP request pipeline.
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        
        app.UseRouting();
        app.UseMiddleware<ExceptionStatusMiddleware>();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}