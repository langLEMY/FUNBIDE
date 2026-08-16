using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Personal;

/// <param name="Permisos">Módulos togglables efectivos del usuario. Solo lo puebla <c>VerPerfilPropioUseCase</c> (mi-perfil); en el resto queda <c>null</c>.</param>
public sealed record UsuarioDto(
    Guid Id,
    string NombreCompleto,
    string Correo,
    string NombreUsuario,
    RolUsuario Rol,
    bool Activo,
    string? FotoPerfilUrl,
    EspecialidadMedica? Especialidad,
    IReadOnlyList<ModuloPermiso>? Permisos = null);
