using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Implementations;
using PRN232.LMS.Repositories.Repositories.Interfaces;

namespace PRN232.LMS.StudentService.Repositories;

public sealed class StudentOnlyRepository : GenericRepository<Student, int>, IStudentRepository
{
    public StudentOnlyRepository(LmsDbContext db)
        : base(db, nameof(Student.StudentId))
    {
    }

    public IQueryable<Student> QueryWithEnrollments(bool asNoTracking = true)
        => Query(asNoTracking);

    public Task<Student?> GetByIdWithEnrollmentsAsync(int id)
        => GetByIdAsync(id);
}
