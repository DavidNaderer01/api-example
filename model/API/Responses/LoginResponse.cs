namespace API.Responses;

public record LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string? RefreshToken { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public int? RefreshExpiresIn { get; init; }
    public string? Scope { get; init; }
}
