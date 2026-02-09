namespace ResponseLibrary.Responses;

public record ClaimInfo
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
