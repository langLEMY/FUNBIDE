using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Personal;

public sealed record UsuarioDto(
    Guid Id,
    string NombreCompleto,
    string Correo,
    RolUsuario Rol,
    bool Activo,
    string? FotoPerfilUrl);
