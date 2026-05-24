namespace PRN232.LMS.Services.Models;

public sealed record EnrollmentModel(
    int EnrollmentId,
    int? StudentId,
    int? CourseId,
    DateTime? EnrollDate,
    string Status,
    StudentSummaryModel? Student,
    CourseSummaryModel? Course);

public sealed record EnrollmentSummaryModel(
    int EnrollmentId,
    DateTime? EnrollDate,
    string Status,
    StudentSummaryModel? Student,
    CourseSummaryModel? Course);

public sealed record EnrollmentUpsertModel(int StudentId, int CourseId, DateTime? EnrollDate, string Status);
