using Grpc.Core;
using PRN232.LMS.Grpc;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.StudentService.Grpc;

public sealed class StudentGrpcService : StudentLookup.StudentLookupBase
{
    private readonly IStudentService _students;

    public StudentGrpcService(IStudentService students)
    {
        _students = students;
    }

    public override async Task<StudentReply> GetStudent(StudentRequest request, ServerCallContext context)
    {
        var student = await _students.GetByIdAsync(request.StudentId);
        if (student is null)
        {
            return new StudentReply { Exists = false };
        }

        return new StudentReply
        {
            Exists = true,
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
        };
    }
}
