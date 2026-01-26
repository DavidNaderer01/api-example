using System.Linq.Expressions;
using Domain.Base;

namespace Repositories.Base.Crud.Methods.SyncMethods;

public interface IReadAll<T>
where T : class, IEntity
{
    IEnumerable<T> All(
            int start = 0,
            int size = 10,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            IEnumerable<Expression<Func<T, object>>>? includes = null,
            bool asNoTracking = true
        );
}