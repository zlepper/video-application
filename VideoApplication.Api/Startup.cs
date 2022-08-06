using Microsoft.OpenApi.Models;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using VideoApplication.Api.Extensions;
using VideoApplication.Api.Middleware;
using VideoApplication.Shared.Setup;
using VideoApplication.Shared.Storage;

namespace VideoApplication.Api;

public class Startup 
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        });
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

        services.AddVideoApplicationDbContext(connectionString, _hostEnvironment.IsDevelopment());
        services.AddS3Storage(_configuration);
        
        services.ConfigureRebus(_configuration.UseRebusRabbitMqTransport(RouteName.Api));

        services.AddApplicationServices();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(p =>
            {
                p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
            });
        });

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
        app.UseCors();
        
        app.UseMiddleware<ExceptionStatusMiddleware>();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}