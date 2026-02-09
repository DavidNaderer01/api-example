namespace ResponseLibrary.Responses;

public record LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public int? RefreshExpiresIn { get; set; }
    public string Scope { get; set; } = string.Empty;
}
