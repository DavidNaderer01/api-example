using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Keycloak.Swagger;

public static class SwaggerExtensions
{
    /// <summary>
    /// Adds JWT Bearer security definition to Swagger
    /// </summary>
    public static void AddSwaggerJwtSecurityDefinition(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Enter your JWT token in the text input below."
            });

            c.OperationFilter<SwaggerSecurityRequrementsOperationFilter>();
        });
    }
}
