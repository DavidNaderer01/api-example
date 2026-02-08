using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Keycloak.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddKeycloakAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationHandler, AuthHandler>();
        
        return services;
    }

    public static AuthorizationPolicyBuilder RequireKeycloakRoles(
        this AuthorizationPolicyBuilder builder, 
        params string[] roles)
    {
        builder.AddRequirements(new AuthRequirement(roles));
        return builder;
    }

    public static void AddKeycloakRolePolicy(
        this AuthorizationOptions options,
        string policyName,
        params string[] roles)
    {
        options.AddPolicy(policyName, policy => 
            policy.Requirements.Add(new AuthRequirement(roles)));
    }
}
