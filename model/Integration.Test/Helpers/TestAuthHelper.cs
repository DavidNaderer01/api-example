using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using API;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TestProject1.Helpers;

/// <summary>
/// Helper methods for creating test users with specific claims
/// </summary>
public static class TestUserHelper
{
    public static ClaimsPrincipal CreateTestUser(
        string username = "testuser",
        string email = "test@example.com",
        string givenName = "Test",
        string familyName = "User",
        params string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("email", email),
            new Claim("given_name", givenName),
            new Claim("family_name", familyName),
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("preferred_username", username)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreateAdminUser()
    {
        return CreateTestUser("admin", "admin@example.com", "Admin", "User", "admin", "user");
    }

    public static ClaimsPrincipal CreateRegularUser()
    {
        return CreateTestUser("user", "user@example.com", "Regular", "User", "user");
    }

    public static ClaimsPrincipal CreateUnauthenticatedUser()
    {
        var identity = new ClaimsIdentity();
        return new ClaimsPrincipal(identity);
    }
}
