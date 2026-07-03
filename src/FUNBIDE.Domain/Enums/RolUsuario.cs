namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Roles reconocidos por el sistema. Deben coincidir exactamente con el claim
/// "role" emitido por Supabase Auth (app_metadata.role).
/// </summary>
public enum RolUsuario
{
    Admin,
    Doctor,
    Fondos,
    Farmacia,

    /// <summary>
    /// Rol de administración del sistema: gestiona el personal (crear/eliminar perfiles,
    /// cambiar roles, restablecer contraseñas, actualizar fotos de perfil). No tiene
    /// acceso a los módulos clínicos ni de inventario.
    /// </summary>
    Lemy
}
