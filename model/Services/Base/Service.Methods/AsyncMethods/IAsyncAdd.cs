using Services.Base.Dto;

namespace Services.Base.Service.Methods.AsyncMethods;

public interface IAsyncAdd<in TDto>
where TDto : class, IDto
{
    Task<Guid> AddAsync(TDto dto);
}