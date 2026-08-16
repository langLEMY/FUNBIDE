namespace FUNBIDE.Application.DTOs.Auth;

/// <summary>Cuerpo JSON enviado por el frontend tras cada intento de inicio de sesión.</summary>
public sealed record RegistrarEventoLoginRequest(string NombreUsuario, bool Exitoso);
