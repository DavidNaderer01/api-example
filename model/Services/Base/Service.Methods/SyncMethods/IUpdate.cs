using Services.Base.Dto;

namespace Services.Base.Service.Methods.SyncMethods;

public interface IUpdate<in T>
    where T : class, IDto
{
    Guid Update(T instance, Guid id);
}