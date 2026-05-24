namespace PRN232.LMS.Services.Models;

public sealed record StudentModel(
    int StudentId,
    string FullName,
    string Email,
    DateTime DateOfBirth,
    IReadOnlyList<EnrollmentSummaryModel>? Enrollments);

public sealed record StudentSummaryModel(int StudentId, string FullName, string Email);

public sealed record StudentUpsertModel(string FullName, string Email, DateTime DateOfBirth);
