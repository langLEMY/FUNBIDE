using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = FUNBIDE.Application.Exceptions.ValidationException;

namespace FUNBIDE.API.Middleware;

/// <summary>
/// Único punto de traducción entre excepciones de Domain/Application y respuestas
/// HTTP. Los controladores no contienen try/catch: dejan que la excepción suba.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status400BadRequest, "Solicitud inválida", ex.Message, ex.Errores);
        }
        catch (RecursoNoEncontradoException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Message);
        }
        catch (OperacionNoPermitidaException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status403Forbidden, "Operación no permitida", ex.Message);
        }
        catch (StockInsuficienteException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Stock insuficiente", ex.Message);
        }
        catch (CorreoEnUsoException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Correo en uso", ex.Message);
        }
        catch (CedulaEnUsoException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Cédula en uso", ex.Message);
        }
        catch (CodigoInventarioEnUsoException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Código en uso", ex.Message);
        }
        catch (DomainException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status400BadRequest, "Regla de negocio violada", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Transición de estado inválida", ex.Message);
        }
        catch (ArgumentException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status400BadRequest, "Argumento inválido", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado procesando {Metodo} {Ruta}", context.Request.Method, context.Request.Path);
            await EscribirProblemaAsync(context, StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.");
        }
    }

    private static async Task EscribirProblemaAsync(
        HttpContext context, int codigoEstado, string titulo, string detalle, object? errores = null)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = codigoEstado;

        var problema = new ProblemDetails
        {
            Title = titulo,
            Detail = detalle,
            Status = codigoEstado,
            Instance = context.Request.Path
        };

        if (errores is not null)
        {
            problema.Extensions["errores"] = errores;
        }

        await context.Response.WriteAsJsonAsync(problema);
    }
}
