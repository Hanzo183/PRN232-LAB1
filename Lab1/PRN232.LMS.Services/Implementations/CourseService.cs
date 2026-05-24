using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Implementations;

public sealed class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;
    private readonly ISemesterRepository _semesters;

    public CourseService(ICourseRepository courses, ISemesterRepository semesters)
    {
        _courses = courses;
        _semesters = semesters;
    }

    public async Task<PagedResult<CourseModel>> GetListAsync(ListQuery query)
    {
        var includeSemester = query.Expand.Contains("semester");
        var includeEnrollments = query.Expand.Contains("enrollments");

        IQueryable<Course> baseQuery = (includeSemester, includeEnrollments) switch
        {
            (true, true) => _courses.QueryWithSemesterAndEnrollments(),
            (true, false) => _courses.QueryWithSemester(),
            (false, true) => _courses.QueryWithEnrollments(),
            _ => _courses.Query(),
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            baseQuery = baseQuery.Where(c => EF.Functions.Like(c.CourseName, $"%{term}%"));
        }

        IOrderedQueryable<Course>? ordered = null;
        foreach (var sort in query.Sort)
        {
            ordered = sort.Field.ToLowerInvariant() switch
            {
                "courseid" => baseQuery.ApplyOrder(ordered, c => c.CourseId, sort.Descending),
                "coursename" => baseQuery.ApplyOrder(ordered, c => c.CourseName, sort.Descending),
                "semesterid" => baseQuery.ApplyOrder(ordered, c => c.SemesterId, sort.Descending),
                _ => ordered
            };
        }

        var finalQuery = (ordered ?? baseQuery.OrderBy(c => c.CourseId));

        var totalItems = await finalQuery.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await finalQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var models = items.Select(c => ToModel(c, includeSemester, includeEnrollments)).ToList();
        return new PagedResult<CourseModel>(models, new PaginationMetadata(page, pageSize, totalItems, totalPages));
    }

    public async Task<CourseModel?> GetByIdAsync(int id)
    {
        var entity = await _courses.GetByIdWithSemesterAndEnrollmentsAsync(id);
        return entity is null ? null : ToModel(entity, includeSemester: true, includeEnrollments: true);
    }

    public async Task<ServiceResult<CourseModel>> CreateAsync(CourseUpsertModel model)
    {
        if (string.IsNullOrWhiteSpace(model.CourseName))
        {
            return ServiceResult<CourseModel>.Fail("Validation", "CourseName is required.");
        }

        if (model.SemesterId is not null)
        {
            var semesterExists = await _semesters.GetByIdAsync(model.SemesterId.Value) is not null;
            if (!semesterExists)
            {
                return ServiceResult<CourseModel>.Fail("Validation", "SemesterId does not exist.");
            }
        }

        var entity = new Course
        {
            CourseName = model.CourseName.Trim(),
            SemesterId = model.SemesterId,
        };

        await _courses.CreateAsync(entity);
        var created = await _courses.GetByIdAsync(entity.CourseId);
        return ServiceResult<CourseModel>.Ok(ToModel(created!, includeSemester: false, includeEnrollments: false));
    }

    public async Task<ServiceResult<CourseModel>> UpdateAsync(int id, CourseUpsertModel model)
    {
        var entity = await _courses.GetForUpdateAsync(id);
        if (entity is null)
        {
            return ServiceResult<CourseModel>.Fail("NotFound", $"Course {id} not found.");
        }

        if (model.SemesterId is not null)
        {
            var semesterExists = await _semesters.GetByIdAsync(model.SemesterId.Value) is not null;
            if (!semesterExists)
            {
                return ServiceResult<CourseModel>.Fail("Validation", "SemesterId does not exist.");
            }
        }

        entity.CourseName = model.CourseName.Trim();
        entity.SemesterId = model.SemesterId;

        await _courses.SaveChangesAsync();

        var updated = await _courses.GetByIdAsync(id);
        return ServiceResult<CourseModel>.Ok(ToModel(updated!, includeSemester: false, includeEnrollments: false));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var deleted = await _courses.DeleteAsync(id);
        return deleted
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Fail("NotFound", $"Course {id} not found.");
    }

    private static CourseModel ToModel(Course entity, bool includeSemester, bool includeEnrollments)
    {
        SemesterSummaryModel? semester = null;
        if (includeSemester && entity.Semester is not null)
        {
            semester = new SemesterSummaryModel(
                entity.Semester.SemesterId,
                entity.Semester.SemesterName,
                entity.Semester.StartDate,
                entity.Semester.EndDate);
        }

        IReadOnlyList<EnrollmentSummaryModel>? enrollments = null;
        if (includeEnrollments)
        {
            enrollments = entity.Enrollments
                .OrderBy(e => e.EnrollmentId)
                .Select(e => new EnrollmentSummaryModel(
                    EnrollmentId: e.EnrollmentId,
                    EnrollDate: e.EnrollDate,
                    Status: e.Status,
                    Student: e.Student is null ? null : new StudentSummaryModel(e.Student.StudentId, e.Student.FullName, e.Student.Email),
                    Course: null))
                .ToList();
        }

        return new CourseModel(
            CourseId: entity.CourseId,
            CourseName: entity.CourseName,
            SemesterId: entity.SemesterId,
            Semester: semester,
            Enrollments: enrollments);
    }
}
