namespace PRN232.LMS.Services.Models;

public sealed record CourseModel(
    int CourseId,
    string CourseName,
    int? SemesterId,
    SemesterSummaryModel? Semester,
    IReadOnlyList<EnrollmentSummaryModel>? Enrollments);

public sealed record CourseSummaryModel(int CourseId, string CourseName, int? SemesterId);

public sealed record CourseUpsertModel(string CourseName, int? SemesterId);
