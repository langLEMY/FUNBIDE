using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
        catch (NombreUsuarioEnUsoException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Nombre de usuario en uso", ex.Message);
        }
        catch (CedulaEnUsoException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Cédula en uso", ex.Message);
        }
        catch (CredencialesInvalidasException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status401Unauthorized, "Credenciales inválidas", ex.Message);
        }
        catch (CodigoInventarioEnUsoException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Código en uso", ex.Message);
        }
        catch (NombreSeguroMedicoEnUsoException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Nombre en uso", ex.Message);
        }
        catch (CodigoServicioEnUsoException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Código en uso", ex.Message);
        }
        catch (HorarioNoDisponibleException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Horario no disponible", ex.Message);
        }
        catch (PacienteConCitaActivaException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Cita activa existente", ex.Message);
        }
        catch (CitaYaCobradaException ex)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Cita ya cobrada", ex.Message);
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
        // Red de seguridad para condiciones de carrera "verificar-luego-insertar": el caso de
        // uso valida y luego inserta sin transacción serializada, así que dos peticiones
        // simultáneas pueden pasar ambas la validación. El índice único de la base es la
        // última línea de defensa real; sin este catch, la segunda petición devolvía 500.
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Ya existe", "El recurso ya existe o entra en conflicto con datos existentes.");
        }
        // Token de concurrencia optimista (xmin) violado: otra petición modificó la misma
        // fila entre que esta la leyó y la guardó (p. ej. dos cierres de caja concurrentes).
        catch (DbUpdateConcurrencyException)
        {
            await EscribirProblemaAsync(context, StatusCodes.Status409Conflict, "Modificado por otra operación", "El recurso fue modificado por otra operación mientras tanto. Recargá e intentá de nuevo.");
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
