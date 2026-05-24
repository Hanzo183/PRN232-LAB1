using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories.Implementations;

public sealed class StudentRepository : GenericRepository<Student, int>, IStudentRepository
{
    private readonly LmsDbContext _db;

    public StudentRepository(LmsDbContext db)
        : base(db, nameof(Student.StudentId))
    {
        _db = db;
    }

    public IQueryable<Student> QueryWithEnrollments(bool asNoTracking = true)
    {
        var query = _db.Students
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .ThenInclude(c => c!.Semester);

        return asNoTracking ? query.AsNoTracking() : query;
    }

    public Task<Student?> GetByIdWithEnrollmentsAsync(int id)
        => QueryWithEnrollments(asNoTracking: true).FirstOrDefaultAsync(s => s.StudentId == id);
}
