using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Implementations;

public sealed class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IStudentLookupClient _students;
    private readonly ICourseRepository _courses;

    public EnrollmentService(IEnrollmentRepository enrollments, IStudentLookupClient students, ICourseRepository courses)
    {
        _enrollments = enrollments;
        _students = students;
        _courses = courses;
    }

    public async Task<PagedResult<EnrollmentModel>> GetListAsync(ListQuery query)
    {
        var includeStudent = query.Expand.Contains("student");
        var includeCourse = query.Expand.Contains("course");

        IQueryable<Enrollment> baseQuery = (includeStudent, includeCourse) switch
        {
            (true, true) => _enrollments.QueryWithStudentAndCourse(),
            (true, false) => _enrollments.QueryWithStudent(),
            (false, true) => _enrollments.QueryWithCourse(),
            _ => _enrollments.Query(),
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            baseQuery = baseQuery.Where(e =>
                EF.Functions.Like(e.Status, $"%{term}%") ||
                (e.Student != null && EF.Functions.Like(e.Student.FullName, $"%{term}%")) ||
                (e.Course != null && EF.Functions.Like(e.Course.CourseName, $"%{term}%")));
        }

        IOrderedQueryable<Enrollment>? ordered = null;
        foreach (var sort in query.Sort)
        {
            ordered = sort.Field.ToLowerInvariant() switch
            {
                "enrollmentid" => baseQuery.ApplyOrder(ordered, e => e.EnrollmentId, sort.Descending),
                "enrolldate" => baseQuery.ApplyOrder(ordered, e => e.EnrollDate, sort.Descending),
                "status" => baseQuery.ApplyOrder(ordered, e => e.Status, sort.Descending),
                "studentid" => baseQuery.ApplyOrder(ordered, e => e.StudentId, sort.Descending),
                "courseid" => baseQuery.ApplyOrder(ordered, e => e.CourseId, sort.Descending),
                _ => ordered
            };
        }

        var finalQuery = (ordered ?? baseQuery.OrderBy(e => e.EnrollmentId));

        var totalItems = await finalQuery.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await finalQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var models = items.Select(e => ToModel(e, includeStudent, includeCourse)).ToList();
        return new PagedResult<EnrollmentModel>(models, new PaginationMetadata(page, pageSize, totalItems, totalPages));
    }

    public async Task<EnrollmentModel?> GetByIdAsync(int id)
    {
        var entity = await _enrollments.GetByIdWithStudentAndCourseAsync(id);
        return entity is null ? null : ToModel(entity, includeStudent: true, includeCourse: true);
    }

    public async Task<ServiceResult<EnrollmentModel>> CreateAsync(EnrollmentUpsertModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Status))
        {
            return ServiceResult<EnrollmentModel>.Fail("Validation", "Status is required.");
        }

        var student = await _students.GetStudentAsync(model.StudentId);
        if (student is null)
        {
            return ServiceResult<EnrollmentModel>.Fail("Validation", "StudentId does not exist.");
        }

        var courseExists = await _courses.GetByIdAsync(model.CourseId) is not null;
        if (!courseExists)
        {
            return ServiceResult<EnrollmentModel>.Fail("Validation", "CourseId does not exist.");
        }

        var entity = new Enrollment
        {
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate ?? DateTime.UtcNow,
            Status = model.Status.Trim(),
        };

        await _enrollments.CreateAsync(entity);
        var created = await _enrollments.GetByIdWithStudentAndCourseAsync(entity.EnrollmentId);
        var modelResult = ToModel(created!, includeStudent: false, includeCourse: true) with { Student = student };
        return ServiceResult<EnrollmentModel>.Ok(modelResult);
    }

    public async Task<ServiceResult<EnrollmentModel>> UpdateAsync(int id, EnrollmentUpsertModel model)
    {
        var entity = await _enrollments.GetForUpdateAsync(id);
        if (entity is null)
        {
            return ServiceResult<EnrollmentModel>.Fail("NotFound", $"Enrollment {id} not found.");
        }

        var student = await _students.GetStudentAsync(model.StudentId);
        if (student is null)
        {
            return ServiceResult<EnrollmentModel>.Fail("Validation", "StudentId does not exist.");
        }

        var courseExists = await _courses.GetByIdAsync(model.CourseId) is not null;
        if (!courseExists)
        {
            return ServiceResult<EnrollmentModel>.Fail("Validation", "CourseId does not exist.");
        }

        entity.StudentId = model.StudentId;
        entity.CourseId = model.CourseId;
        entity.EnrollDate = model.EnrollDate;
        entity.Status = model.Status.Trim();

        await _enrollments.SaveChangesAsync();

        var updated = await _enrollments.GetByIdWithStudentAndCourseAsync(id);
        var modelResult = ToModel(updated!, includeStudent: false, includeCourse: true) with { Student = student };
        return ServiceResult<EnrollmentModel>.Ok(modelResult);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var deleted = await _enrollments.DeleteAsync(id);
        return deleted
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Fail("NotFound", $"Enrollment {id} not found.");
    }

    private static EnrollmentModel ToModel(Enrollment entity, bool includeStudent, bool includeCourse)
    {
        var student = includeStudent && entity.Student is not null
            ? new StudentSummaryModel(entity.Student.StudentId, entity.Student.FullName, entity.Student.Email)
            : null;

        var course = includeCourse && entity.Course is not null
            ? new CourseSummaryModel(entity.Course.CourseId, entity.Course.CourseName, entity.Course.SemesterId)
            : null;

        return new EnrollmentModel(
            EnrollmentId: entity.EnrollmentId,
            StudentId: entity.StudentId,
            CourseId: entity.CourseId,
            EnrollDate: entity.EnrollDate,
            Status: entity.Status,
            Student: student,
            Course: course);
    }
}
