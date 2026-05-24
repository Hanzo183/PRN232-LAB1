using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories.Implementations;

public sealed class EnrollmentRepository : GenericRepository<Enrollment, int>, IEnrollmentRepository
{
    private readonly LmsDbContext _db;

    public EnrollmentRepository(LmsDbContext db)
        : base(db, nameof(Enrollment.EnrollmentId))
    {
        _db = db;
    }

    public IQueryable<Enrollment> QueryWithStudent(bool asNoTracking = true)
    {
        var query = _db.Enrollments.Include(e => e.Student);
        return asNoTracking ? query.AsNoTracking() : query;
    }

    public IQueryable<Enrollment> QueryWithCourse(bool asNoTracking = true)
    {
        var query = _db.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c!.Semester);

        return asNoTracking ? query.AsNoTracking() : query;
    }

    public IQueryable<Enrollment> QueryWithStudentAndCourse(bool asNoTracking = true)
    {
        var query = _db.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ThenInclude(c => c!.Semester);

        return asNoTracking ? query.AsNoTracking() : query;
    }

    public Task<Enrollment?> GetByIdWithStudentAndCourseAsync(int id)
        => QueryWithStudentAndCourse(asNoTracking: true).FirstOrDefaultAsync(e => e.EnrollmentId == id);
}
