using System.Linq.Expressions;
using Domain.Base;

namespace Repositories.Base.Crud.Methods.AsyncMethods;

public interface IAsyncRead<T>
where T : class, IEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetFirstAsync(
        Expression<Func<T, bool>>? filter = null,
        IEnumerable<Expression<Func<T, object>>>? includes = null,
        bool asNoTracking = true
    );
}