namespace API.Extentions;

public static class CorsExtentsions
{
    public static IServiceCollection AddSwaggerCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSwaggerUI", builder =>
            {
                builder.WithOrigins(
                                "http://localhost:5110",
                                "https://localhost:7098"
                            )
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });
        return services;
    }
}
