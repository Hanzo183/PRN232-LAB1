namespace PRN232.LMS.Services.Querying;

public sealed class ListQuery
{
    public string? Search { get; init; }
    public IReadOnlyList<SortDescriptor> Sort { get; init; } = Array.Empty<SortDescriptor>();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public IReadOnlySet<string> Expand { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
