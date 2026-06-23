using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentLookupClient
{
    Task<StudentSummaryModel?> GetStudentAsync(int studentId, CancellationToken cancellationToken = default);
}
