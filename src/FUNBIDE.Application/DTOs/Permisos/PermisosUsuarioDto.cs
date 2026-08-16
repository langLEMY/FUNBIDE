using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Permisos;

public sealed record PermisosUsuarioDto(
    Guid UsuarioId,
    string NombreCompleto,
    RolUsuario Rol,
    IReadOnlyList<ModuloPermisoEstadoDto> Modulos);
