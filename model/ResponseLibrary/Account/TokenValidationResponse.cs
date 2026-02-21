namespace ResponseLibrary.Account;

public record TokenValidationResponse
{
    public bool IsValid { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
    public string IssuedAt { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
}
