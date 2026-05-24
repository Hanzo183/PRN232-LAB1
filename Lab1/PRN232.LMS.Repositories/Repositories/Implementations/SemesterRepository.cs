using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories.Implementations;

public sealed class SemesterRepository : GenericRepository<Semester, int>, ISemesterRepository
{
    private readonly LmsDbContext _db;

    public SemesterRepository(LmsDbContext db)
        : base(db, nameof(Semester.SemesterId))
    {
        _db = db;
    }

    public IQueryable<Semester> QueryWithCourses(bool asNoTracking = true)
    {
        var query = _db.Semesters.Include(s => s.Courses);
        return asNoTracking ? query.AsNoTracking() : query;
    }

    public Task<Semester?> GetByIdWithCoursesAsync(int id)
        => QueryWithCourses(asNoTracking: true).FirstOrDefaultAsync(s => s.SemesterId == id);
}
