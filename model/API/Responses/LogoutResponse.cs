namespace API.Responses;

public record LogoutResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
