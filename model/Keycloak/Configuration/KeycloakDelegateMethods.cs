using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Keycloak.Configuration;

public static class KeycloakDelegateMethods
{
    /// <summary>
    /// Handles authentication failures
    /// </summary>
    public static Task OnAuthenticationFailedAsync(AuthenticationFailedContext context)
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
    public static Task OnChallengeAsync(JwtBearerChallengeContext context)
    {
        var logger = context.HttpContext.RequestServices
            .GetService<ILogger<JwtBearerHandler>>();

        logger?.LogWarning("JWT Bearer challenge: {Error}, {ErrorDescription}",
            context.Error,
            context.ErrorDescription);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles token validation and role mapping
    /// </summary>
    public static Task OnTokenValidatedAsync(TokenValidatedContext context)
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
    /// Handles incoming token messages
    /// </summary>
    public static Task OnMessageReceivedAsync(MessageReceivedContext context)
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
}
