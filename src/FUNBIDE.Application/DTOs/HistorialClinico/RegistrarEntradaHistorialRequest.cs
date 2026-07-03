namespace FUNBIDE.Application.DTOs.HistorialClinico;

/// <summary>
/// <paramref name="Contenido"/> viaja como <see cref="System.Text.Json.JsonElement"/>
/// para permitir una estructura clínica flexible que se persiste tal cual en la
/// columna JSONB, sin forzar un esquema rígido en la capa de aplicación.
/// </summary>
public sealed record RegistrarEntradaHistorialRequest(
    Guid PacienteId,
    Guid? CitaId,
    System.Text.Json.JsonElement Contenido);
