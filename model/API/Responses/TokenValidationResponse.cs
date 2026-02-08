namespace API.Responses;

public record TokenValidationResponse
{
    public bool IsValid { get; init; }
    public string? Username { get; init; }
    public string? ExpiresAt { get; init; }
    public string? IssuedAt { get; init; }
    public string? Issuer { get; init; }
}
