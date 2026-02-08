using API.Requests;
using System.Text.Json.Serialization;

namespace API.Responses;

internal record KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("refresh_expires_in")]
    public int? RefreshExpiresIn { get; init; }

    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

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
