namespace PRN232.LMS.API.Models.Responses;

public sealed record SemesterSummaryResponse(int SemesterId, string SemesterName, DateTime StartDate, DateTime EndDate);

public sealed record SemesterResponse(
    int SemesterId,
    string SemesterName,
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyList<CourseSummaryResponse>? Courses);
