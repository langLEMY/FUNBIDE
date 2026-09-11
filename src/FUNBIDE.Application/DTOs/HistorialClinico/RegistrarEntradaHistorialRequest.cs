using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.HistorialClinico;

/// <summary>
/// <paramref name="Contenido"/> viaja como <see cref="System.Text.Json.JsonElement"/>
/// para permitir una estructura clínica flexible que se persiste tal cual en la
/// columna JSONB, sin forzar un esquema rígido en la capa de aplicación — la forma
/// esperada depende de <paramref name="Tipo"/> (nota libre, receta con ítems, o
/// documento con cuerpo de texto; ver ContenidoHistorial en el frontend).
/// </summary>
public sealed record RegistrarEntradaHistorialRequest(
    Guid PacienteId,
    Guid? CitaId,
    TipoEntradaHistorial Tipo,
    System.Text.Json.JsonElement Contenido);
