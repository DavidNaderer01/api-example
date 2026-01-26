using System.Linq.Expressions;
using Domain.Base;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Base;

public abstract class BaseRepository<TEntity>(DbContext context) :
    IBaseRepository<TEntity>
    where TEntity : class, IEntity
{
    protected readonly DbContext Context = context;
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    #region Sync Methods

    public Guid Create(TEntity instance)
    {
        DbSet.Add(instance);
        Context.SaveChanges();
        return instance.Id;
    }

    public TEntity GetById(Guid id)
    {
        return DbSet.Find(id) ?? throw new KeyNotFoundException($"Entity with id {id} not found.");
    }

    public TEntity GetFirst(
        Expression<Func<TEntity, bool>>? filter = null, 
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null, 
        bool asNoTracking = true)
    {
        IQueryable<TEntity> query = DbSet;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (filter != null)
            query = query.Where(filter);

        return query.FirstOrDefault() ?? throw new KeyNotFoundException("Entity not found.");
    }

    public IEnumerable<TEntity> All(
        int start = 0, 
        int size = 10, 
        Expression<Func<TEntity, bool>>? filter = null, 
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null, 
        bool asNoTracking = true)
    {
        IQueryable<TEntity> query = DbSet;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = orderBy(query);

        return query.Skip(start).Take(size).ToList();
    }

    public Guid Update(TEntity instance)
    {
        DbSet.Update(instance);
        Context.SaveChanges();
        return instance.Id;
    }

    public bool Delete(Guid id)
    {
        var entity = DbSet.Find(id);
        if (entity == null)
            return false;

        DbSet.Remove(entity);
        Context.SaveChanges();
        return true;
    }

    #endregion

    #region Async Methods

    public async Task<Guid> CreateAsync(TEntity instance)
    {
        await DbSet.AddAsync(instance);
        await Context.SaveChangesAsync();
        return instance.Id;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id) ?? throw new KeyNotFoundException($"Entity with id {id} not found.");
    }

    public async Task<TEntity?> GetFirstAsync(
        Expression<Func<TEntity, bool>>? filter = null, 
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null, 
        bool asNoTracking = true)
    {
        IQueryable<TEntity> query = DbSet;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (filter != null)
            query = query.Where(filter);

        return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Entity not found.");
    }

    public async IAsyncEnumerable<TEntity> AllAsync(
        int start = 0, 
        int size = 10, 
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, 
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        bool asNoTracking = true)
    {
        IQueryable<TEntity> query = DbSet;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = orderBy(query);

        await foreach (var entity in query.Skip(start).Take(size).AsAsyncEnumerable())
        {
            yield return entity;
        }
    }

    public async Task<Guid> UpdateAsync(TEntity instance)
    {
        DbSet.Update(instance);
        await Context.SaveChangesAsync();
        return instance.Id;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity == null)
            return false;

        DbSet.Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }

    #endregion
}