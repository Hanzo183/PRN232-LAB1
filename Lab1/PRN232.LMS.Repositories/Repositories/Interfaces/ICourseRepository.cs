using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories.Interfaces;

public interface ICourseRepository : IGenericRepository<Course, int>
{
    IQueryable<Course> QueryWithSemester(bool asNoTracking = true);
    IQueryable<Course> QueryWithEnrollments(bool asNoTracking = true);
    IQueryable<Course> QueryWithSemesterAndEnrollments(bool asNoTracking = true);
    Task<Course?> GetByIdWithSemesterAndEnrollmentsAsync(int id);
}
