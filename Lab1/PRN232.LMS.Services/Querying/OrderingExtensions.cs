using System.Linq.Expressions;

namespace PRN232.LMS.Services.Querying;

public static class OrderingExtensions
{
    public static IOrderedQueryable<T> ApplyOrder<T, TKey>(
        this IQueryable<T> query,
        IOrderedQueryable<T>? ordered,
        Expression<Func<T, TKey>> keySelector,
        bool descending)
    {
        if (ordered is null)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
    }
}
