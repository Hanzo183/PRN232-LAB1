namespace PRN232.LMS.API.Models;

public sealed record Pagination(int Page, int PageSize, int TotalItems, int TotalPages);

public sealed record PagedData<T>(IReadOnlyList<T> Items, Pagination Pagination);
