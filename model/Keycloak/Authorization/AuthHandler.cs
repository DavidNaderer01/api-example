using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Keycloak.Authorization;

public class AuthHandler : AuthorizationHandler<AuthRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        AuthRequirement requirement)
    {
        if (requirement.RequiredRoles == null || requirement.RequiredRoles.Length == 0)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userRoles = context.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (requirement.RequiredRoles.Any(role => userRoles.Contains(role)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
