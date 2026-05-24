namespace PRN232.LMS.API.Models.Responses;

public sealed record CourseSummaryResponse(int CourseId, string CourseName, int? SemesterId);

public sealed record CourseResponse(
    int CourseId,
    string CourseName,
    int? SemesterId,
    SemesterSummaryResponse? Semester,
    IReadOnlyList<EnrollmentSummaryResponse>? Enrollments);
