using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories.Interfaces;

public interface ISemesterRepository : IGenericRepository<Semester, int>
{
    IQueryable<Semester> QueryWithCourses(bool asNoTracking = true);
    Task<Semester?> GetByIdWithCoursesAsync(int id);
}
