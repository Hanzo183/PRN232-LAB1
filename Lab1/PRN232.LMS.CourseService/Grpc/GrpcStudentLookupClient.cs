using Grpc.Core;
using PRN232.LMS.Grpc;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.CourseService.Grpc;

public sealed class GrpcStudentLookupClient : IStudentLookupClient
{
    private readonly StudentLookup.StudentLookupClient _client;
    private readonly ILogger<GrpcStudentLookupClient> _logger;

    public GrpcStudentLookupClient(StudentLookup.StudentLookupClient client, ILogger<GrpcStudentLookupClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<StudentSummaryModel?> GetStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reply = await _client.GetStudentAsync(new StudentRequest { StudentId = studentId }, cancellationToken: cancellationToken);
            return reply.Exists
                ? new StudentSummaryModel(reply.StudentId, reply.FullName, reply.Email)
                : null;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Student gRPC lookup failed for StudentId {StudentId}", studentId);
            return null;
        }
    }
}
