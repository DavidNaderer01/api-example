using API.Controllers;
using Keycloak.Swagger;
using Serilog;
using Services.Account;
using Services.Redis;

namespace API.Extentions;

public static class ServiceExtentions
{
    public static void AddHostServices(this IServiceCollection services)
    {
        try
        {
            services.AddControllers();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerJwtSecurityDefinition();
            services.AddHttpClient();

            Log.Information("Services added successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding services");
            throw;
        }
    }

    public static void AddCustomServices(this IServiceCollection services)
    {
        try
        {
            services.AddTransient<IAccountService, AccountService<AccountController>>();
            services.AddSingleton<IRedisCacheService, RedisCacheService>();

            Log.Information("Custom services added successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding custom services");
            throw;
        }
    }
}
