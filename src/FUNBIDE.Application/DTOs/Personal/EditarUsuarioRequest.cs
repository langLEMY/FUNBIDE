namespace FUNBIDE.Application.DTOs.Personal;

public sealed record EditarUsuarioRequest(Guid UsuarioId, string NombreCompleto, string Correo);
