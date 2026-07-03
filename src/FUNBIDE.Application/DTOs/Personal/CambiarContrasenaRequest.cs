namespace FUNBIDE.Application.DTOs.Personal;

public sealed record CambiarContrasenaRequest(Guid UsuarioId, string NuevaContrasena);
