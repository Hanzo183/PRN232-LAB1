using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Infrastructure;

public static class Mapping
{
    public static StudentSummaryResponse ToResponse(this StudentSummaryModel model)
        => new(model.StudentId, model.FullName, model.Email);

    public static CourseSummaryResponse ToResponse(this CourseSummaryModel model)
        => new(model.CourseId, model.CourseName, model.SemesterId);

    public static SemesterSummaryResponse ToResponse(this SemesterSummaryModel model)
        => new(model.SemesterId, model.SemesterName, model.StartDate, model.EndDate);

    public static EnrollmentSummaryResponse ToResponse(this EnrollmentSummaryModel model)
        => new(
            model.EnrollmentId,
            model.EnrollDate,
            model.Status,
            model.Student is null ? null : model.Student.ToResponse(),
            model.Course is null ? null : model.Course.ToResponse());

    public static StudentResponse ToResponse(this StudentModel model)
        => new(
            model.StudentId,
            model.FullName,
            model.Email,
            model.DateOfBirth,
            model.Enrollments?.Select(e => e.ToResponse()).ToList());

    public static CourseResponse ToResponse(this CourseModel model)
        => new(
            model.CourseId,
            model.CourseName,
            model.SemesterId,
            model.Semester is null ? null : model.Semester.ToResponse(),
            model.Enrollments?.Select(e => e.ToResponse()).ToList());

    public static SemesterResponse ToResponse(this SemesterModel model)
        => new(
            model.SemesterId,
            model.SemesterName,
            model.StartDate,
            model.EndDate,
            model.Courses?.Select(c => c.ToResponse()).ToList());

    public static SubjectResponse ToResponse(this SubjectModel model)
        => new(model.SubjectId, model.SubjectCode, model.SubjectName, model.Credit);

    public static EnrollmentResponse ToResponse(this EnrollmentModel model)
        => new(
            model.EnrollmentId,
            model.StudentId,
            model.CourseId,
            model.EnrollDate,
            model.Status,
            model.Student is null ? null : model.Student.ToResponse(),
            model.Course is null ? null : model.Course.ToResponse());
}
