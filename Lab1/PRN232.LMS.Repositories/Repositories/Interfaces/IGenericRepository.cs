using System.Linq.Expressions;

namespace PRN232.LMS.Repositories.Repositories.Interfaces;

public interface IGenericRepository<TEntity, in TKey>
    where TEntity : class
{
    IQueryable<TEntity> Query(bool asNoTracking = true);

    Task<TEntity?> GetByIdAsync(TKey id);

    Task<TEntity?> GetForUpdateAsync(TKey id);

    Task<TEntity> CreateAsync(TEntity entity);

    Task<bool> DeleteAsync(TKey id);

    Task<int> SaveChangesAsync();

    // Optional helper for ad-hoc queries without exposing DbContext.
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = true);
}
