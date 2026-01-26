using System.Linq.Expressions;
using Domain.Base;
using Services.Base.Dto;

namespace Services.Base.Service.Methods.SyncMethods;

public interface IGet<TEntity, out TDto>
    where TEntity : class, IEntity
    where TDto : class, IDto
{
    TDto GetById(Guid id);
    TDto GetFirst(
        Expression<Func<TEntity, bool>>? filter = null,
        IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
        bool asNoTracking = true
    );
}

