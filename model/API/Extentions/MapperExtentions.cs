using AutoMapper;
using Mapper.UserInfoMapping;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extentions;

public static class MapperExtentions
{
    public static void AddAutoMappers(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => {
            cfg.AddProfile<UserInfoMapper>();
        });
    }
}
