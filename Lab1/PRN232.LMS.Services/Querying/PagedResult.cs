namespace PRN232.LMS.Services.Querying;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, PaginationMetadata Pagination);
