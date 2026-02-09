using Keycloak.Configuration;
using Serilog;

namespace API.Extentions;

public static class AuthenticationExtentions
{
    public static void AddJwtAuthentication(
        this IServiceCollection services,
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
        catch (Exception ex)
        {
            Log.Error(ex, "Error configuring Keycloak authentication");
            throw;
        }
    }
}
