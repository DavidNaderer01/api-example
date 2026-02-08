namespace API.Responses;

public record UserInfoResponse
{
    public string? Username { get; init; }
    public bool IsAuthenticated { get; init; }
    public string? AuthenticationType { get; init; }
    public string? Email { get; init; }
    public string? GivenName { get; init; }
    public string? FamilyName { get; init; }
    public List<string> Roles { get; init; } = new();
    public List<ClaimInfo> Claims { get; init; } = new();
}
