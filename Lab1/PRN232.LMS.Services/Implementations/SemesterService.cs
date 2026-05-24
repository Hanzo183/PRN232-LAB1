using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Implementations;

public sealed class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesters;

    public SemesterService(ISemesterRepository semesters)
    {
        _semesters = semesters;
    }

    public async Task<PagedResult<SemesterModel>> GetListAsync(ListQuery query)
    {
        var includeCourses = query.Expand.Contains("courses");
        IQueryable<Semester> baseQuery = includeCourses
            ? _semesters.QueryWithCourses()
            : _semesters.Query();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            baseQuery = baseQuery.Where(s => EF.Functions.Like(s.SemesterName, $"%{term}%"));
        }

        IOrderedQueryable<Semester>? ordered = null;
        foreach (var sort in query.Sort)
        {
            ordered = sort.Field.ToLowerInvariant() switch
            {
                "semesterid" => baseQuery.ApplyOrder(ordered, s => s.SemesterId, sort.Descending),
                "semestername" => baseQuery.ApplyOrder(ordered, s => s.SemesterName, sort.Descending),
                "startdate" => baseQuery.ApplyOrder(ordered, s => s.StartDate, sort.Descending),
                "enddate" => baseQuery.ApplyOrder(ordered, s => s.EndDate, sort.Descending),
                _ => ordered
            };
        }

        var finalQuery = (ordered ?? baseQuery.OrderBy(s => s.SemesterId));

        var totalItems = await finalQuery.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await finalQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var models = items.Select(s => ToModel(s, includeCourses)).ToList();
        return new PagedResult<SemesterModel>(models, new PaginationMetadata(page, pageSize, totalItems, totalPages));
    }

    public async Task<SemesterModel?> GetByIdAsync(int id)
    {
        var entity = await _semesters.GetByIdWithCoursesAsync(id);
        return entity is null ? null : ToModel(entity, includeCourses: true);
    }

    public async Task<ServiceResult<SemesterModel>> CreateAsync(SemesterUpsertModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SemesterName))
        {
            return ServiceResult<SemesterModel>.Fail("Validation", "SemesterName is required.");
        }

        if (model.EndDate < model.StartDate)
        {
            return ServiceResult<SemesterModel>.Fail("Validation", "EndDate must be after StartDate.");
        }

        var entity = new Semester
        {
            SemesterName = model.SemesterName.Trim(),
            StartDate = model.StartDate,
            EndDate = model.EndDate,
        };

        await _semesters.CreateAsync(entity);
        var created = await _semesters.GetByIdAsync(entity.SemesterId);
        return ServiceResult<SemesterModel>.Ok(ToModel(created!, includeCourses: false));
    }

    public async Task<ServiceResult<SemesterModel>> UpdateAsync(int id, SemesterUpsertModel model)
    {
        var entity = await _semesters.GetForUpdateAsync(id);
        if (entity is null)
        {
            return ServiceResult<SemesterModel>.Fail("NotFound", $"Semester {id} not found.");
        }

        if (model.EndDate < model.StartDate)
        {
            return ServiceResult<SemesterModel>.Fail("Validation", "EndDate must be after StartDate.");
        }

        entity.SemesterName = model.SemesterName.Trim();
        entity.StartDate = model.StartDate;
        entity.EndDate = model.EndDate;

        await _semesters.SaveChangesAsync();

        var updated = await _semesters.GetByIdAsync(id);
        return ServiceResult<SemesterModel>.Ok(ToModel(updated!, includeCourses: false));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var deleted = await _semesters.DeleteAsync(id);
        return deleted
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Fail("NotFound", $"Semester {id} not found.");
    }

    private static SemesterModel ToModel(Semester entity, bool includeCourses)
    {
        IReadOnlyList<CourseSummaryModel>? courses = null;
        if (includeCourses)
        {
            courses = entity.Courses
                .OrderBy(c => c.CourseId)
                .Select(c => new CourseSummaryModel(c.CourseId, c.CourseName, c.SemesterId))
                .ToList();
        }

        return new SemesterModel(
            SemesterId: entity.SemesterId,
            SemesterName: entity.SemesterName,
            StartDate: entity.StartDate,
            EndDate: entity.EndDate,
            Courses: courses);
    }
}
