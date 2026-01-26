using System.Linq.Expressions;
using AutoMapper;
using Domain.Base;
using Repositories.Base;
using Services.Base.Dto;

namespace Services.Base;

public abstract class BaseService<TEntity, TDto>(IBaseRepository<TEntity> repository, IMapper mapper)
    : IBaseService<TEntity, TDto>
    where TEntity : class, IEntity
    where TDto : class, IDto
{
    protected readonly IBaseRepository<TEntity> Repository = repository;
    protected readonly IMapper Mapper = mapper;

    #region Sync Methods

    public Guid Add(TDto dto)
    {
        var entity = Mapper.Map<TEntity>(dto);
        return Repository.Create(entity);
    }

    public TDto GetById(Guid id)
    {
        var entity = Repository.GetById(id);
        return Mapper.Map<TDto>(entity);
    }

    public TDto GetFirst(
        Expression<Func<TEntity, bool>>? filter = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        bool asNoTracking = true)
    {
        var entity = Repository.GetFirst(filter, includes, asNoTracking);
        return Mapper.Map<TDto>(entity);
    }

    public IEnumerable<TDto> All(
        int start = 0,
        int size = 10,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        bool asNoTracking = true)
    {
        var entities = Repository.All(start, size, filter, orderBy, includes, asNoTracking);
        return Mapper.Map<IEnumerable<TDto>>(entities);
    }

    public Guid Update(TDto dto, Guid id)
    {
        var entity = Mapper.Map<TEntity>(dto);
        entity.Id = id;
        return Repository.Update(entity);
    }

    public bool Delete(Guid id)
    {
        return Repository.Delete(id);
    }

    #endregion

    #region Async Methods

    public async Task<Guid> AddAsync(TDto dto)
    {
        var entity = Mapper.Map<TEntity>(dto);
        return await Repository.CreateAsync(entity);
    }

    public async Task<TDto> GetByIdAsync(Guid id)
    {
        var entity = await Repository.GetByIdAsync(id);
        return Mapper.Map<TDto>(entity);
    }

    public async Task<TDto> GetFirstAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        bool asNoTracking = true)
    {
        var entity = await Repository.GetFirstAsync(filter, includes, asNoTracking);
        return Mapper.Map<TDto>(entity);
    }

    public async IAsyncEnumerable<TDto> AllAsync(
        int start = 0,
        int size = 10,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        bool asNoTracking = true)
    {
        await foreach (var entity in Repository.AllAsync(start, size, filter, orderBy, includes, asNoTracking))
        {
            yield return Mapper.Map<TDto>(entity);
        }
    }

    public async Task<Guid> UpdateAsync(TDto dto, Guid id)
    {
        var entity = Mapper.Map<TEntity>(dto);
        entity.Id = id;
        return await Repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await Repository.DeleteAsync(id);
    }

    #endregion
}
