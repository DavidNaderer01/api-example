using System.Linq.Expressions;
using Domain.Base;

namespace Repositories.Base.Crud.Methods.AsyncMethods;

public interface IAsyncReadAll<T> 
    where T : class, IEntity
{
    IAsyncEnumerable<T> AllAsync(
        int start = 0,
        int size = 10,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        IEnumerable<Expression<Func<T, object>>>? includes = null,
        bool asNoTracking = true
    );
}