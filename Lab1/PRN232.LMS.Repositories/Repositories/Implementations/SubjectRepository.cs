using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories.Implementations;

public sealed class SubjectRepository : GenericRepository<Subject, int>, ISubjectRepository
{
    public SubjectRepository(LmsDbContext db)
        : base(db, nameof(Subject.SubjectId))
    {
    }
}
