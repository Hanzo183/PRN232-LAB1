using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Implementations;

public sealed class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjects;

    public SubjectService(ISubjectRepository subjects)
    {
        _subjects = subjects;
    }

    public async Task<PagedResult<SubjectModel>> GetListAsync(ListQuery query)
    {
        IQueryable<Subject> baseQuery = _subjects.Query();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            baseQuery = baseQuery.Where(s =>
                EF.Functions.Like(s.SubjectCode, $"%{term}%") ||
                EF.Functions.Like(s.SubjectName, $"%{term}%"));
        }

        IOrderedQueryable<Subject>? ordered = null;
        foreach (var sort in query.Sort)
        {
            ordered = sort.Field.ToLowerInvariant() switch
            {
                "subjectid" => baseQuery.ApplyOrder(ordered, s => s.SubjectId, sort.Descending),
                "subjectcode" => baseQuery.ApplyOrder(ordered, s => s.SubjectCode, sort.Descending),
                "subjectname" => baseQuery.ApplyOrder(ordered, s => s.SubjectName, sort.Descending),
                "credit" => baseQuery.ApplyOrder(ordered, s => s.Credit, sort.Descending),
                _ => ordered
            };
        }

        var finalQuery = (ordered ?? baseQuery.OrderBy(s => s.SubjectId));

        var totalItems = await finalQuery.CountAsync();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await finalQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var models = items.Select(ToModel).ToList();
        return new PagedResult<SubjectModel>(models, new PaginationMetadata(page, pageSize, totalItems, totalPages));
    }

    public async Task<SubjectModel?> GetByIdAsync(int id)
    {
        var entity = await _subjects.GetByIdAsync(id);
        return entity is null ? null : ToModel(entity);
    }

    public async Task<ServiceResult<SubjectModel>> CreateAsync(SubjectUpsertModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SubjectCode) || string.IsNullOrWhiteSpace(model.SubjectName))
        {
            return ServiceResult<SubjectModel>.Fail("Validation", "SubjectCode and SubjectName are required.");
        }

        var entity = new Subject
        {
            SubjectCode = model.SubjectCode.Trim(),
            SubjectName = model.SubjectName.Trim(),
            Credit = model.Credit,
        };

        await _subjects.CreateAsync(entity);
        var created = await _subjects.GetByIdAsync(entity.SubjectId);
        return ServiceResult<SubjectModel>.Ok(ToModel(created!));
    }

    public async Task<ServiceResult<SubjectModel>> UpdateAsync(int id, SubjectUpsertModel model)
    {
        var entity = await _subjects.GetForUpdateAsync(id);
        if (entity is null)
        {
            return ServiceResult<SubjectModel>.Fail("NotFound", $"Subject {id} not found.");
        }

        entity.SubjectCode = model.SubjectCode.Trim();
        entity.SubjectName = model.SubjectName.Trim();
        entity.Credit = model.Credit;

        await _subjects.SaveChangesAsync();

        var updated = await _subjects.GetByIdAsync(id);
        return ServiceResult<SubjectModel>.Ok(ToModel(updated!));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var deleted = await _subjects.DeleteAsync(id);
        return deleted
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Fail("NotFound", $"Subject {id} not found.");
    }

    private static SubjectModel ToModel(Subject entity)
        => new(entity.SubjectId, entity.SubjectCode, entity.SubjectName, entity.Credit);
}
