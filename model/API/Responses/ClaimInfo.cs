namespace API.Responses;

public record ClaimInfo
{
    public string Type { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}
