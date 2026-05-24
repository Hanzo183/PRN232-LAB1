namespace PRN232.LMS.API.Models.Responses;

public sealed record StudentSummaryResponse(int StudentId, string FullName, string Email);

public sealed record StudentResponse(
    int StudentId,
    string FullName,
    string Email,
    DateTime DateOfBirth,
    IReadOnlyList<EnrollmentSummaryResponse>? Enrollments);
