using System.Reflection;

namespace PRN232.LMS.API.Infrastructure;

public static class FieldSelection
{
    public static (bool Success, string? Error, IReadOnlyList<object> Items) Shape<T>(
        IReadOnlyList<T> items,
        IReadOnlyList<string>? fields)
        where T : class
    {
        if (fields is null || fields.Count == 0)
        {
            return (true, null, items.Cast<object>().ToList());
        }

        var props = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        foreach (var f in fields)
        {
            if (!props.ContainsKey(f))
            {
                return (false, $"Invalid field '{f}'.", Array.Empty<object>());
            }
        }

        var shaped = new List<object>(items.Count);
        foreach (var item in items)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in fields)
            {
                var prop = props[f];
                dict[prop.Name] = prop.GetValue(item);
            }
            shaped.Add(dict);
        }

        return (true, null, shaped);
    }
}
