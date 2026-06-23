using PRN232.LMS.Repositories.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public sealed class RepositoryStudentLookupClient : IStudentLookupClient
{
    private readonly IStudentRepository _students;

    public RepositoryStudentLookupClient(IStudentRepository students)
    {
        _students = students;
    }

    public async Task<StudentSummaryModel?> GetStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var student = await _students.GetByIdAsync(studentId);
        return student is null
            ? null
            : new StudentSummaryModel(student.StudentId, student.FullName, student.Email);
    }
}
