using FUNBIDE.API.Middleware;

namespace FUNBIDE.API.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Orden deliberado: el manejo de excepciones envuelve todo; los encabezados
    /// reenviados por Nginx se aplican antes que nada más para que la IP real del
    /// cliente esté disponible en el resto del pipeline; el guard append-only y la
    /// validación de roles corren después de enrutar y autenticar (necesitan el
    /// endpoint resuelto y el ClaimsPrincipal poblado) pero antes de ejecutar la acción.
    /// </summary>
    public static IApplicationBuilder UseFunbidePipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseForwardedHeaders();

        app.UseRouting();
        app.UseRateLimiter();
        app.UseCors(FUNBIDE.Infrastructure.Security.CorsExtensions.PoliticaFrontend);

        app.UseAuthentication();
        app.UseMiddleware<RequestAuditLoggingMiddleware>();
        app.UseMiddleware<AppendOnlyGuardMiddleware>();
        app.UseMiddleware<RoleAuthorizationMiddleware>();
        app.UseMiddleware<MantenimientoMiddleware>();
        app.UseAuthorization();

        return app;
    }
}
