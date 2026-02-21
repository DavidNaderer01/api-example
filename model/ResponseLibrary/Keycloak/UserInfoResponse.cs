using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;

namespace ResponseLibrary.Keycloak;

public record UserInfoResponse
{
    public string Username { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
    public string AuthenticationType { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public ICollection<string> Roles { get; set; } = [];
    public ICollection<ClaimInfo> Claims { get; set; } = [];
}
