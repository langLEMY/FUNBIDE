using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FUNBIDE.Infrastructure.Security;

public static class CorsExtensions
{
    public const string PoliticaFrontend = "FrontendFunbide";

    /// <summary>
    /// Restringe CORS a los orígenes explícitos configurados en "Cors:OrigenesPermitidos".
    /// Nunca usa AllowAnyOrigin: el backend maneja datos clínicos sensibles.
    /// </summary>
    public static IServiceCollection AddFunbideCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origenesPermitidos = configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(PoliticaFrontend, builder =>
            {
                builder
                    .WithOrigins(origenesPermitidos)
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PATCH", "DELETE")
                    .AllowCredentials();
            });
        });

        return services;
    }
}
