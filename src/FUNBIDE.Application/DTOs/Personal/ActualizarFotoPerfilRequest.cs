namespace FUNBIDE.Application.DTOs.Personal;

public sealed record ActualizarFotoPerfilRequest(Guid UsuarioId, Stream Contenido, string ContentType);
