using Services.Base.Dto;

namespace Services.Base.Service.Methods.SyncMethods;

public interface IAdd<in TDto>
where TDto : class, IDto
{
    Guid Add(TDto dto);
}