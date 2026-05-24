using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Querying;
using PRN232.LMS.Services.Results;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<PagedResult<EnrollmentModel>> GetListAsync(ListQuery query);
    Task<EnrollmentModel?> GetByIdAsync(int id);

    Task<ServiceResult<EnrollmentModel>> CreateAsync(EnrollmentUpsertModel model);
    Task<ServiceResult<EnrollmentModel>> UpdateAsync(int id, EnrollmentUpsertModel model);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
