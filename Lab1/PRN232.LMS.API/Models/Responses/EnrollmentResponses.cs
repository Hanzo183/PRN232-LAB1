namespace PRN232.LMS.API.Models.Responses;

public sealed record EnrollmentResponse(
    int EnrollmentId,
    int? StudentId,
    int? CourseId,
    DateTime? EnrollDate,
    string Status,
    StudentSummaryResponse? Student,
    CourseSummaryResponse? Course);

public sealed record EnrollmentSummaryResponse(
    int EnrollmentId,
    DateTime? EnrollDate,
    string Status,
    StudentSummaryResponse? Student,
    CourseSummaryResponse? Course);
