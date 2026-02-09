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
            ValidAudiences = new[] { clientId, "account" },
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
            OnMessageReceived = OnMessageReceivedAsync,
            OnTokenValidated = OnTokenValidatedAsync,
            OnAuthenticationFailed = OnAuthenticationFailedAsync,
            OnChallenge = OnChallengeAsync
        };
    }

    /// <summary>
    /// Handles incoming token messages
    /// </summary>
    private static Task OnMessageReceivedAsync(MessageReceivedContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetService<ILogger<JwtBearerHandler>>();
        
        var token = context.Request.Headers.Authorization.FirstOrDefault();
        
        if (!string.IsNullOrEmpty(token))
        {
            var tokenValue = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? token.Substring(7)
                : token;
            
            context.Token = tokenValue;
        }
        else
        {
            logger?.LogWarning("No Authorization header found in request!");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles token validation and role mapping
    /// </summary>
    private static Task OnTokenValidatedAsync(TokenValidatedContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetService<ILogger<JwtBearerHandler>>();
        
        if (context.Principal?.Identity is ClaimsIdentity identity)
        {
            MapRealmRolesToClaims(identity, context.SecurityToken);
            
            var roles = identity.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            logger?.LogInformation("JWT Bearer authenticated successfully!");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Maps Keycloak realm roles to ClaimTypes.Role claims
    /// </summary>
    private static void MapRealmRolesToClaims(ClaimsIdentity identity, SecurityToken? securityToken)
    {
        if (securityToken is not Microsoft.IdentityModel.JsonWebTokens.JsonWebToken token)
            return;

        if (!token.TryGetPayloadValue("realm_access", out object? realmAccess))
            return;

        if (realmAccess is not System.Text.Json.JsonElement realmElement)
            return;

        if (!realmElement.TryGetProperty("roles", out var rolesElement))
            return;

        if (rolesElement.ValueKind != System.Text.Json.JsonValueKind.Array)
            return;

        foreach (var role in rolesElement.EnumerateArray())
        {
            var roleValue = role.GetString();
            if (!string.IsNullOrEmpty(roleValue))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
            }
        }
    }

    /// <summary>
    /// Handles authentication failures
    /// </summary>
    private static Task OnAuthenticationFailedAsync(AuthenticationFailedContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetService<ILogger<JwtBearerHandler>>();
        
        logger?.LogError("JWT Bearer authentication failed: {Error}", context.Exception.Message);
        
        if (context.Exception is SecurityTokenInvalidAudienceException)
        {
            logger?.LogWarning("Audience validation failed. Check if token audience matches configured client ID or 'account'");
        }
        else if (context.Exception.Message.Contains("Unable to decode"))
        {
            logger?.LogWarning("Token decode failed. Ensure token is correctly formatted (no 'Bearer ' prefix in token itself)");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles authentication challenges
    /// </summary>
    private static Task OnChallengeAsync(JwtBearerChallengeContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetService<ILogger<JwtBearerHandler>>();
        
        logger?.LogWarning("JWT Bearer challenge: {Error}, {ErrorDescription}",
            context.Error,
            context.ErrorDescription);
        
        return Task.CompletedTask;
    }
}