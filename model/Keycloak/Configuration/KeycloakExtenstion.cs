using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Keycloak.Authorization;

namespace Keycloak.Configuration;

public static class KeycloakExtension
{
    /// <summary>
    /// Adds JWT Bearer authentication with Keycloak
    /// </summary>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        string keycloakUrl,
        string realm,
        string clientId,
        bool includeAuthorization = true,
        bool requireHttpsMetadata = false
        )
    {
        var authority = $"{keycloakUrl.TrimEnd('/')}/realms/{realm}";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.RequireHttpsMetadata = requireHttpsMetadata;
            options.TokenValidationParameters = CreateTokenValidationParameters(authority, clientId);
            options.Events = CreateJwtBearerEvents();
        });

        if (includeAuthorization)
        {
            services.AddKeycloakAuthorization();
        }

        return services;
    }

    /// <summary>
    /// Adds JWT Bearer authentication with Keycloak using options
    /// </summary>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        KeycloakOptions keycloakOptions)
    {
        return services.AddKeycloakAuthentication(
            keycloakOptions.KeycloakUrl,
            keycloakOptions.Realm,
            keycloakOptions.ClientId,
            keycloakOptions.IncludeAuthorization
        );
    }

    /// <summary>
    /// Configures Keycloak authentication with custom authorization policies
    /// </summary>
    public static IServiceCollection AddKeycloakAuthenticationWithPolicies(
        this IServiceCollection services,
        string keycloakUrl,
        string realm,
        string clientId,
        Action<AuthorizationOptions>? configureAuthorization = null
        )
    {
        services.AddKeycloakAuthentication(
            keycloakUrl,
            realm,
            clientId,
            includeAuthorization: false
        );

        services.AddAuthorization(options =>
        {
            configureAuthorization?.Invoke(options);
        });
        
        services.AddSingleton<IAuthorizationHandler, AuthHandler>();

        return services;
    }

    /// <summary>
    /// Creates token validation parameters for Keycloak JWT tokens
    /// </summary>
    private static TokenValidationParameters CreateTokenValidationParameters(string authority, string clientId)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authority,
            ValidAudiences = [clientId, "account"],
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    }

    /// <summary>
    /// Creates JWT Bearer events for token handling
    /// </summary>
    private static JwtBearerEvents CreateJwtBearerEvents()
    {
        return new JwtBearerEvents
        {
            OnMessageReceived = KeycloakDelegateMethods.OnMessageReceivedAsync,
            OnTokenValidated = KeycloakDelegateMethods.OnTokenValidatedAsync,
            OnAuthenticationFailed = KeycloakDelegateMethods.OnAuthenticationFailedAsync,
            OnChallenge = KeycloakDelegateMethods.OnChallengeAsync
        };
    }
}