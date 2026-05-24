using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories.Implementations;

public abstract class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly DbContext Db;
    private readonly string _keyPropertyName;

    protected GenericRepository(DbContext db, string keyPropertyName)
    {
        Db = db;
        _keyPropertyName = keyPropertyName;
    }

    protected DbSet<TEntity> Set => Db.Set<TEntity>();

    public virtual IQueryable<TEntity> Query(bool asNoTracking = true)
        => asNoTracking ? Set.AsNoTracking() : Set;

    public virtual Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = true)
        => (asNoTracking ? Set.AsNoTracking() : Set).FirstOrDefaultAsync(predicate);

    public virtual Task<TEntity?> GetByIdAsync(TKey id)
        => Query(asNoTracking: true).FirstOrDefaultAsync(BuildKeyPredicate(id));

    public virtual Task<TEntity?> GetForUpdateAsync(TKey id)
        => Set.FirstOrDefaultAsync(BuildKeyPredicate(id));

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        Set.Add(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(TKey id)
    {
        var entity = await GetForUpdateAsync(id);
        if (entity is null)
        {
            return false;
        }

        Set.Remove(entity);
        await Db.SaveChangesAsync();
        return true;
    }

    public Task<int> SaveChangesAsync() => Db.SaveChangesAsync();

    private Expression<Func<TEntity, bool>> BuildKeyPredicate(TKey id)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");

        var efPropertyMethod = typeof(EF)
            .GetMethods()
            .Single(m => m.Name == nameof(EF.Property) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TKey));

        var left = Expression.Call(
            efPropertyMethod,
            parameter,
            Expression.Constant(_keyPropertyName));

        var right = Expression.Constant(id, typeof(TKey));
        var body = Expression.Equal(left, right);

        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }
}
