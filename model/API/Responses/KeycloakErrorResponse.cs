using System.Text.Json.Serialization;

namespace API.Responses;

internal record KeycloakErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }
}
