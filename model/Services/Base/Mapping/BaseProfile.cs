using AutoMapper;
using Domain.Base;

namespace Services.Base.Mapping;

public abstract class BaseProfile<TEntity, TDto> : Profile
    where TEntity : class, IEntity
    where TDto : class
{
    protected BaseProfile()
    {
        CreateMap<TEntity, TDto>();
        CreateMap<TDto, TEntity>();
    }
}

