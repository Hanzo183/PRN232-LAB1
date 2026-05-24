namespace PRN232.LMS.Services.Models;

public sealed record SemesterModel(
    int SemesterId,
    string SemesterName,
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyList<CourseSummaryModel>? Courses);

public sealed record SemesterSummaryModel(int SemesterId, string SemesterName, DateTime StartDate, DateTime EndDate);

public sealed record SemesterUpsertModel(string SemesterName, DateTime StartDate, DateTime EndDate);
