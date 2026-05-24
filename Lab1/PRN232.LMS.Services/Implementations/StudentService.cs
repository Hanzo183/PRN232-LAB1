using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Implementations;

public sealed class StudentService : IStudentService
{
    private readonly IStudentRepository _students;

    public StudentService(IStudentRepository students)
    {
        _students = students;
    }

    public async Task<PagedResult<StudentModel>> GetListAsync(ListQuery query)
    {
        var includeEnrollments = query.Expand.Contains("enrollments");
        IQueryable<Student> baseQuery = includeEnrollments
            ? _students.QueryWithEnrollments()
            : _students.Query();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            baseQuery = baseQuery.Where(s =>
                EF.Functions.Like(s.FullName, $"%{term}%") ||
                EF.Functions.Like(s.Email, $"%{term}%"));
        }

        IOrderedQueryable<Student>? ordered = null;
        foreach (var sort in query.Sort)
        {
            ordered = sort.Field.ToLowerInvariant() switch
            {
                "studentid" => baseQuery.ApplyOrder(ordered, s => s.StudentId, sort.Descending),
                "fullname" => baseQuery.ApplyOrder(ordered, s => s.FullName, sort.Descending),
                "email" => baseQuery.ApplyOrder(ordered, s => s.Email, sort.Descending),
                "dateofbirth" => baseQuery.ApplyOrder(ordered, s => s.DateOfBirth, sort.Descending),
                _ => ordered
            };
        }

        var finalQuery = (ordered ?? baseQuery.OrderBy(s => s.StudentId));

        var totalItems = await finalQuery.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await finalQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var models = items.Select(s => ToModel(s, includeEnrollments)).ToList();
        return new PagedResult<StudentModel>(models, new PaginationMetadata(page, pageSize, totalItems, totalPages));
    }

    public async Task<StudentModel?> GetByIdAsync(int id)
    {
        var entity = await _students.GetByIdWithEnrollmentsAsync(id);
        return entity is null ? null : ToModel(entity, includeEnrollments: true);
    }

    public async Task<ServiceResult<StudentModel>> CreateAsync(StudentUpsertModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
        {
            return ServiceResult<StudentModel>.Fail("Validation", "FullName and Email are required.");
        }

        var entity = new Student
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            DateOfBirth = model.DateOfBirth,
        };

        await _students.CreateAsync(entity);
        var created = await _students.GetByIdAsync(entity.StudentId);
        return ServiceResult<StudentModel>.Ok(ToModel(created!, includeEnrollments: false));
    }

    public async Task<ServiceResult<StudentModel>> UpdateAsync(int id, StudentUpsertModel model)
    {
        var entity = await _students.GetForUpdateAsync(id);
        if (entity is null)
        {
            return ServiceResult<StudentModel>.Fail("NotFound", $"Student {id} not found.");
        }

        entity.FullName = model.FullName.Trim();
        entity.Email = model.Email.Trim();
        entity.DateOfBirth = model.DateOfBirth;

        await _students.SaveChangesAsync();

        var updated = await _students.GetByIdAsync(id);
        return ServiceResult<StudentModel>.Ok(ToModel(updated!, includeEnrollments: false));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var deleted = await _students.DeleteAsync(id);
        return deleted
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Fail("NotFound", $"Student {id} not found.");
    }

    private static StudentModel ToModel(Student entity, bool includeEnrollments)
    {
        IReadOnlyList<EnrollmentSummaryModel>? enrollments = null;
        if (includeEnrollments)
        {
            enrollments = entity.Enrollments
                .OrderBy(e => e.EnrollmentId)
                .Select(e => new EnrollmentSummaryModel(
                    EnrollmentId: e.EnrollmentId,
                    EnrollDate: e.EnrollDate,
                    Status: e.Status,
                    Student: null,
                    Course: e.Course is null ? null : new CourseSummaryModel(e.Course.CourseId, e.Course.CourseName, e.Course.SemesterId)))
                .ToList();
        }

        return new StudentModel(
            StudentId: entity.StudentId,
            FullName: entity.FullName,
            Email: entity.Email,
            DateOfBirth: entity.DateOfBirth,
            Enrollments: enrollments);
    }
}
