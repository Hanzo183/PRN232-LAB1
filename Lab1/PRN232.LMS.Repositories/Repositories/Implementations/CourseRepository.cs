using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories.Implementations;

public sealed class CourseRepository : GenericRepository<Course, int>, ICourseRepository
{
    private readonly LmsDbContext _db;

    public CourseRepository(LmsDbContext db)
        : base(db, nameof(Course.CourseId))
    {
        _db = db;
    }

    public IQueryable<Course> QueryWithSemester(bool asNoTracking = true)
    {
        var query = _db.Courses.Include(c => c.Semester);
        return asNoTracking ? query.AsNoTracking() : query;
    }

    public IQueryable<Course> QueryWithEnrollments(bool asNoTracking = true)
    {
        var query = _db.Courses
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.Student);

        return asNoTracking ? query.AsNoTracking() : query;
    }

    public IQueryable<Course> QueryWithSemesterAndEnrollments(bool asNoTracking = true)
    {
        var query = _db.Courses
            .Include(c => c.Semester)
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.Student);

        return asNoTracking ? query.AsNoTracking() : query;
    }

    public Task<Course?> GetByIdWithSemesterAndEnrollmentsAsync(int id)
        => QueryWithSemesterAndEnrollments(asNoTracking: true).FirstOrDefaultAsync(c => c.CourseId == id);
}
