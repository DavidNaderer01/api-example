using Mapper.UserInfoMapping;

namespace API.Extentions;

public static class MapperExtentions
{
    public static void AddAutoMappers(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(UserInfoMapper));
    }
}
