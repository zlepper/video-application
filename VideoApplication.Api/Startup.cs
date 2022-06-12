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
        
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}