using FUNBIDE.API.Authorization;
using FUNBIDE.Domain.Exceptions;

namespace FUNBIDE.API.Middleware;

/// <summary>
/// Aplica la regla append-only a nivel de transporte HTTP: si el endpoint resuelto
/// está marcado con <see cref="SoloLecturaEInsercionAttribute"/> y el verbo es
/// PUT, PATCH o DELETE, la petición nunca llega al controlador.
/// </summary>
public sealed class AppendOnlyGuardMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> VerbosBloqueados = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Put, HttpMethods.Patch, HttpMethods.Delete
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var esAppendOnly = context.GetEndpoint()?.Metadata.GetMetadata<SoloLecturaEInsercionAttribute>() is not null;

        if (esAppendOnly && VerbosBloqueados.Contains(context.Request.Method))
        {
            throw OperacionNoPermitidaException.RecursoSoloLecturaEInsercion(context.Request.Path);
        }

        await next(context);
    }
}
