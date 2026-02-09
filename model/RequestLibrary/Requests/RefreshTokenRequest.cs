namespace RequestLibrary.Requests;

public record RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
