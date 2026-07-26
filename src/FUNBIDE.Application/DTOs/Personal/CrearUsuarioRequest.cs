using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Personal;

public sealed record CrearUsuarioRequest(
    string NombreCompleto,
    string Correo,
    string ContrasenaTemporal,
    RolUsuario Rol,
    EspecialidadMedica? Especialidad);
