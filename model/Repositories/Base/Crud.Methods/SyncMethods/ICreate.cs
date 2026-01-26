using Domain.Base;

namespace Repositories.Base.Crud.Methods.SyncMethods;

public interface ICreate<in T>
where T : class, IEntity
{
    Guid Create(T instance);
}