namespace PRN232.LMS.API.Models;

public sealed class ListQueryParameters
{
    public string? Search { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public string? Fields { get; init; }
    public string? Expand { get; init; }
}
