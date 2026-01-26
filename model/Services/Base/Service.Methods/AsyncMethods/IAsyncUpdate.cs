using Services.Base.Dto;

namespace Services.Base.Service.Methods.AsyncMethods;

public interface IAsyncUpdate<in TDto>
where TDto : class, IDto
{
    Task<Guid> UpdateAsync(TDto dto, Guid id);
}