using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<PagedResult<SemesterModel>> GetListAsync(ListQuery query);
    Task<SemesterModel?> GetByIdAsync(int id);

    Task<ServiceResult<SemesterModel>> CreateAsync(SemesterUpsertModel model);
    Task<ServiceResult<SemesterModel>> UpdateAsync(int id, SemesterUpsertModel model);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
