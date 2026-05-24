using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories.Interfaces;

public interface IEnrollmentRepository : IGenericRepository<Enrollment, int>
{
    IQueryable<Enrollment> QueryWithStudent(bool asNoTracking = true);
    IQueryable<Enrollment> QueryWithCourse(bool asNoTracking = true);
    IQueryable<Enrollment> QueryWithStudentAndCourse(bool asNoTracking = true);
    Task<Enrollment?> GetByIdWithStudentAndCourseAsync(int id);
}
