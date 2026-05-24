using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<PagedResult<StudentModel>> GetListAsync(ListQuery query);
    Task<StudentModel?> GetByIdAsync(int id);

    Task<ServiceResult<StudentModel>> CreateAsync(StudentUpsertModel model);
    Task<ServiceResult<StudentModel>> UpdateAsync(int id, StudentUpsertModel model);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
