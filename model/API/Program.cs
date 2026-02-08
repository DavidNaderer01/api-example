using Keycloak.Configuration;
using Keycloak.Swagger;
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

        AddServices(builder.Services, configuration);
        AddAuthentication(
            builder.Services, 
            configuration,
            !isDevelopment
        );

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            });
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static void AddAuthentication(
        IServiceCollection services, 
        ConfigurationManager configuration,
        bool requireHttpsMetadata)
    {
        try
        {
            services.AddKeycloakAuthentication(
                configuration["Keycloak:Url"] ?? throw new ArgumentNullException(nameof(configuration), "Keycloak:Url is required"),
                configuration["Keycloak:Realm"] ?? throw new ArgumentNullException(nameof(configuration), "Keycloak:Realm is required"),
                configuration["Keycloak:ClientId"] ?? throw new ArgumentNullException(nameof(configuration), "Keycloak:ClientId is required"),
                true,
                requireHttpsMetadata
            );
            
            Log.Information("Keycloak JWT authentication configured successfully");
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Error configuring Keycloak authentication");
            throw;
        }
    }

    private static void AddServices(IServiceCollection services, ConfigurationManager configuration)
    {
        try
        {
            services.AddControllers();
            services.AddOpenApi();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerJwtSecurityDefinition();
            services.AddHttpClient();
            
            Log.Information("Services added successfully");
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Error adding services");
            throw;
        }  
    }
}
