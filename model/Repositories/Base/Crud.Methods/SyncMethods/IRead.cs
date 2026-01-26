using System.Linq.Expressions;
using Domain.Base;

namespace Repositories.Base.Crud.Methods.SyncMethods;

public interface IRead<T>
where T : class, IEntity
{
    T? GetById(Guid id);
    T? GetFirst(
            Expression<Func<T, bool>>? filter = null,
            IEnumerable<Expression<Func<T, object>>>? includes = null,
            bool asNoTracking = true
        );
}