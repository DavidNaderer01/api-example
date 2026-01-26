using Domain.Base;

namespace Repositories.Base.Crud.Methods.AsyncMethods;

public interface IAsyncUpdate<in T>
    where T : class, IEntity
{
    Task<Guid> UpdateAsync(T instance);
}