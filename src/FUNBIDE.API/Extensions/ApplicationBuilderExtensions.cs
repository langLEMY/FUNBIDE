using FUNBIDE.API.Middleware;

namespace FUNBIDE.API.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Orden deliberado: el manejo de excepciones envuelve todo; el guard append-only
    /// y la validación de roles corren después de enrutar y autenticar (necesitan el
    /// endpoint resuelto y el ClaimsPrincipal poblado) pero antes de ejecutar la acción.
    /// </summary>
    public static IApplicationBuilder UseFunbidePipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseRouting();
        app.UseCors(FUNBIDE.Infrastructure.Security.CorsExtensions.PoliticaFrontend);

        app.UseAuthentication();
        app.UseMiddleware<RequestAuditLoggingMiddleware>();
        app.UseMiddleware<AppendOnlyGuardMiddleware>();
        app.UseMiddleware<RoleAuthorizationMiddleware>();
        app.UseAuthorization();

        return app;
    }
}
