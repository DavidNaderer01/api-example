using Domain.Base;

namespace Repositories.Base.Crud.Methods.AsyncMethods;

public interface IAsyncCreate<in T>
where T : class, IEntity
{
    Task<Guid> CreateAsync(T instance);
}