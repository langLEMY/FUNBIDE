using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Personal;

public sealed record CambiarRolRequest(Guid UsuarioId, RolUsuario NuevoRol);
