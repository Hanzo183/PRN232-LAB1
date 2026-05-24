namespace PRN232.LMS.Services.Querying;

public sealed record PaginationMetadata(int Page, int PageSize, int TotalItems, int TotalPages);
