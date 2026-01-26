using Domain.Base;
using Services.Base.Dto;
using Services.Base.Service.Methods.AsyncMethods;
using Services.Base.Service.Methods.SyncMethods;

namespace Services.Base;

public interface IBaseService<TEntity, TDto> :
    IAdd<TDto>,
    IGet<TEntity, TDto>,
    IAll<TEntity, TDto>,
    IUpdate<TDto>,
    IRemove,
    IAsyncAdd<TDto>,
    IAsyncGet<TEntity, TDto>,
    IAsyncAll<TEntity, TDto>,
    IAsyncUpdate<TDto>,
    IAsyncRemove
    where TEntity : class, IEntity
    where TDto : class, IDto
{
}