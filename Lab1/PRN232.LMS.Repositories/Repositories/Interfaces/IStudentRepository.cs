using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories.Interfaces;

public interface IStudentRepository : IGenericRepository<Student, int>
{
    IQueryable<Student> QueryWithEnrollments(bool asNoTracking = true);
    Task<Student?> GetByIdWithEnrollmentsAsync(int id);
}
