using Domain.Base;

namespace Repositories.Base.Crud.Methods.SyncMethods;

public interface IUpdate<in T>
where T : class, IEntity
{
    Guid Update(T instance);
}