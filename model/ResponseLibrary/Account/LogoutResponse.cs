namespace ResponseLibrary.Account;

public record LogoutResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
