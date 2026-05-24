using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<PagedResult<SubjectModel>> GetListAsync(ListQuery query);
    Task<SubjectModel?> GetByIdAsync(int id);

    Task<ServiceResult<SubjectModel>> CreateAsync(SubjectUpsertModel model);
    Task<ServiceResult<SubjectModel>> UpdateAsync(int id, SubjectUpsertModel model);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
