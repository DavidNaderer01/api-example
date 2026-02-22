using Serilog;
using StackExchange.Redis;

namespace API.Extentions;

public static class RedisExtensions
{
    /// <summary>
    /// Adds Redis distributed caching to the application
    /// </summary>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        try
        {
            var redisEnabled = configuration.GetValue<bool>("Redis:Enabled");
            
            if (!redisEnabled)
            {
                Log.Information("Redis is disabled. Using in-memory cache instead.");
                services.AddDistributedMemoryCache();
                return services;
            }

            var connectionString = configuration["Redis:ConnectionString"]
                ?? throw new ArgumentNullException("Redis:ConnectionString", "Redis connection string is required");
            
            var instanceName = configuration["Redis:InstanceName"] ?? "API:";

            AddExchangeRedisCache(services, connectionString, instanceName);
            AddConnectionMuliplexer(services, connectionString);

            Log.Information("Redis cache configured successfully with connection: {ConnectionString}", connectionString);
            
            return services;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error configuring Redis cache");
            Log.Warning("Falling back to in-memory cache due to Redis configuration error");
            services.AddDistributedMemoryCache();
            
            return services;
        }
    }

    private static void AddConnectionMuliplexer(
            IServiceCollection services,
            string connectionString
        )
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configOptions = ConfigurationOptions.Parse(connectionString);
            configOptions.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configOptions);
        });
    }

    private static void AddExchangeRedisCache(
            IServiceCollection services,
            string connectionString,
            string instanceName
        )
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = instanceName;
        });
    }
}
