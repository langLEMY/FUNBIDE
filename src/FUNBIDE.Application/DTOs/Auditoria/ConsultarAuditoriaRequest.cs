namespace FUNBIDE.Application.DTOs.Auditoria;

/// <summary>
/// Pagina/TamanoPagina son opcionales: si el cliente no los manda (como hoy hace
/// ActividadPage.tsx/MiPerfilPage.tsx en el frontend), ObtenerLogsAuditoriaUseCase sigue
/// devolviendo todo el rango de fechas de una sola vez, igual que siempre — la protección
/// real contra "traer todo por accidente" es el rango de fechas (con default y span
/// máximo), no forzar paginación a un frontend que hoy no tiene controles de "página
/// siguiente" para esta pantalla.
/// </summary>
public sealed record ConsultarAuditoriaRequest(
    DateTimeOffset? Desde, DateTimeOffset? Hasta, string? Recurso, int? Pagina = null, int? TamanoPagina = null);
