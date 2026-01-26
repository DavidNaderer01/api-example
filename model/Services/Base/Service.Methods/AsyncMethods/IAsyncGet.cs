using System.Linq.Expressions;
using Domain.Base;
using Services.Base.Dto;

namespace Services.Base.Service.Methods.AsyncMethods;

public interface IAsyncGet<TEntity, TDto>
    where TEntity : class, IEntity
    where TDto : class, IDto
{
    Task<TDto> GetByIdAsync(Guid id);
    Task<TDto> GetFirstAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        bool asNoTracking = true
    );
}
