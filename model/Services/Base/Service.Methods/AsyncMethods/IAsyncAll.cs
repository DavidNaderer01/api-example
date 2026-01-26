using System.Linq.Expressions;
using Domain.Base;
using Services.Base.Dto;

namespace Services.Base.Service.Methods.AsyncMethods;

public interface IAsyncAll<TEntity, TDto>
    where TEntity : class, IEntity
    where TDto : class, IDto
{
    IAsyncEnumerable<TDto> AllAsync(
        int start = 0,
        int size = 10,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        bool asNoTracking = true
    );
}
