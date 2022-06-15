using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
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
        services.AddSwaggerGen(options =>
        {
            options.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
            options.SchemaGeneratorOptions.UseInlineDefinitionsForEnums = true;
            options.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;
            
            options.AddSecurityDefinition("AccessKeyAuth", new OpenApiSecurityScheme()
            {
                Name = "AccessKey",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
            });
            
            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "AccessKeyAuth",
                    },
                }, Array.Empty<string>()}
            });
            
        });
        services.AddSingleton<ExceptionStatusMiddleware>();
        services.AddStackExchangeRedisCache(o =>
        {
            o.Configuration = _configuration.GetConnectionString("Redis");
        });
        services.AddCustomIdentity();

        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        services.AddVideoApplicationDbContext(connectionString);
        
        services.ConfigureRebus(_configuration, RouteName.Api);

        services.AddApplicationServices();

    }

    public void Configure(IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        // Configure the HTTP request pipeline.
        if (environment.IsDevelopment())
        {
            app.UseSwagger(c => { c.RouteTemplate = "api-docs/{documentName}/swagger.json"; });
            app.UseSwaggerUI(o =>
            {
                o.ConfigObject.PersistAuthorization = true;
                o.RoutePrefix = "api-docs";
            });
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