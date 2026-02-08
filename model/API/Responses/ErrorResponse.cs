namespace API.Responses;

public record ErrorResponse
{
    public string Error { get; init; } = string.Empty;
    public string ErrorDescription { get; init; } = string.Empty;
}
