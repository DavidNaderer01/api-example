using System.Text.Json.Serialization;

namespace ResponseLibrary.Keycloak;

public record KeycloakErrorResponse
{
    [JsonPropertyName("error")] public string Error { get; set; } = string.Empty;

    [JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; } = string.Empty;
}
