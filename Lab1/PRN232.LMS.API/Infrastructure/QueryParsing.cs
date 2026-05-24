using PRN232.LMS.API.Models;
using PRN232.LMS.Services.Querying;

namespace PRN232.LMS.API.Infrastructure;

public static class QueryParsing
{
    public static (bool Success, string? Error, ListQuery? Query) ToListQuery(
        ListQueryParameters input,
        IReadOnlySet<string> allowedSortFields,
        IReadOnlySet<string> allowedExpandValues)
    {
        var page = input.Page < 1 ? 1 : input.Page;
        var size = input.Size <= 0 ? 20 : input.Size;

        if (size > 200)
        {
            return (false, "size must be <= 200", null);
        }

        var sortDescriptors = new List<SortDescriptor>();
        if (!string.IsNullOrWhiteSpace(input.Sort))
        {
            var parts = input.Sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                var desc = part.StartsWith('-');
                var field = desc ? part[1..] : part;

                if (!allowedSortFields.Contains(field))
                {
                    return (false, $"Invalid sort field '{field}'. Allowed: {string.Join(",", allowedSortFields)}", null);
                }

                sortDescriptors.Add(new SortDescriptor(field, desc));
            }
        }

        var expand = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(input.Expand))
        {
            var parts = input.Expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                if (!allowedExpandValues.Contains(part))
                {
                    return (false, $"Invalid expand value '{part}'. Allowed: {string.Join(",", allowedExpandValues)}", null);
                }

                expand.Add(part);
            }
        }

        return (true, null, new ListQuery
        {
            Search = input.Search,
            Sort = sortDescriptors,
            Page = page,
            PageSize = size,
            Expand = expand,
        });
    }

    public static IReadOnlyList<string>? ParseFields(string? fieldsCsv)
    {
        if (string.IsNullOrWhiteSpace(fieldsCsv))
        {
            return null;
        }

        return fieldsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
