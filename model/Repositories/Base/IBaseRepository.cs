using Domain.Base;
using Repositories.Base.Crud.Methods.AsyncMethods;
using Repositories.Base.Crud.Methods.SyncMethods;

namespace Repositories.Base;

public interface IBaseRepository<TEntity> :
    ICreate<TEntity>,
    IRead<TEntity>,
    IReadAll<TEntity>,
    IUpdate<TEntity>,
    IDelete,
    IAsyncCreate<TEntity>,
    IAsyncRead<TEntity>,
    IAsyncReadAll<TEntity>,
    IAsyncUpdate<TEntity>,
    IAsyncDelete
    where TEntity : class, IEntity
{
}

