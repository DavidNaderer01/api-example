using API.Extentions;
using Serilog;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        var configuration = builder.Configuration;
        bool isDevelopment = builder.Environment.IsDevelopment();
        bool isTesting = builder.Environment.EnvironmentName == "Testing";

        builder.Services.AddSwaggerCors();
        builder.Services.AddHostServices();
        builder.Services.AddCustomServices();
        builder.Services.AddAutoMappers();
        builder.Services.AddWebApiVersioning();
        builder.Services.AddRedisCache(configuration);

        if (!isTesting)
        {
            builder.Services.AddJwtAuthentication(
                configuration,
                !isDevelopment
            );
        }

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowSwaggerUI");
        app.UseAuthentication();
        
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
