using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<PagedResult<CourseModel>> GetListAsync(ListQuery query);
    Task<CourseModel?> GetByIdAsync(int id);

    Task<ServiceResult<CourseModel>> CreateAsync(CourseUpsertModel model);
    Task<ServiceResult<CourseModel>> UpdateAsync(int id, CourseUpsertModel model);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
