namespace FUNBIDE.Application.DTOs.Auth;

/// <summary>
/// Versión enriquecida de <see cref="RegistrarEventoLoginRequest"/> con la IP y el
/// dispositivo que el controlador extrae de la petición HTTP misma (nunca del cuerpo
/// enviado por el cliente, para que no se puedan falsificar).
/// </summary>
public sealed record RegistrarEventoLoginComando(string Correo, bool Exitoso, string Ip, string Dispositivo);
