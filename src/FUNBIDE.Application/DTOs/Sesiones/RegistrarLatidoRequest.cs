namespace FUNBIDE.Application.DTOs.Sesiones;

/// <summary>
/// <see cref="SessionId"/> es un id aleatorio que el frontend genera una sola vez por
/// dispositivo (no por pestaña) y guarda en localStorage — no es el JWT en sí.
/// </summary>
public sealed record RegistrarLatidoRequest(string SessionId);
