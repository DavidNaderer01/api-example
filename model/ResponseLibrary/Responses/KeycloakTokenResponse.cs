namespace ResponseLibrary.Responses;

using System.Text.Json.Serialization;

public record KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int? RefreshExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    public LoginResponse Parse()
    {
        return new LoginResponse
        {
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            TokenType = TokenType,
            ExpiresIn = ExpiresIn,
            RefreshExpiresIn = RefreshExpiresIn,
            Scope = Scope
        };
    }
}
