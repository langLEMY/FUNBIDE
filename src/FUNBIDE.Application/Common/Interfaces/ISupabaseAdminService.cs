using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Abstrae las llamadas administrativas a Supabase Auth (GoTrue Admin API) necesarias
/// para que LEMY pueda gestionar el personal: el rol efectivo de un usuario y su
/// contraseña viven en Supabase, no en la base de datos de FUNBIDE, así que cualquier
/// cambio debe reflejarse ahí para que el próximo JWT que emita Supabase lo refleje.
/// </summary>
public interface ISupabaseAdminService
{
    /// <summary>Crea el usuario en Supabase Auth y le asigna app_metadata.role = rol.</summary>
    Task<Guid> CrearUsuarioAsync(
        string correo, string contrasenaTemporal, RolUsuario rol, CancellationToken cancellationToken);

    /// <summary>Actualiza app_metadata.role, que es lo que el JWT proyecta como claim de rol.</summary>
    Task CambiarRolAsync(Guid supabaseUserId, RolUsuario nuevoRol, CancellationToken cancellationToken);

    Task CambiarContrasenaAsync(Guid supabaseUserId, string nuevaContrasena, CancellationToken cancellationToken);

    /// <summary>Actualiza el correo de inicio de sesión (ya confirmado, sin flujo de verificación).</summary>
    Task CambiarCorreoAsync(Guid supabaseUserId, string nuevoCorreo, CancellationToken cancellationToken);

    /// <summary>
    /// Revoca el acceso del usuario en Supabase Auth (no puede volver a iniciar sesión).
    /// La proyección local en "usuarios" se conserva desactivada, no se borra, para no
    /// perder la referencia en citas/historial/movimientos ya registrados con su id.
    /// </summary>
    Task RevocarAccesoAsync(Guid supabaseUserId, CancellationToken cancellationToken);

    /// <summary>Deshace <see cref="RevocarAccesoAsync"/>: el usuario puede volver a iniciar sesión.</summary>
    Task RestaurarAccesoAsync(Guid supabaseUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Borra el usuario de Supabase Auth de forma irreversible: a diferencia de
    /// <see cref="RevocarAccesoAsync"/>, esto no se puede deshacer con
    /// <see cref="RestaurarAccesoAsync"/> — el usuario deja de existir en Supabase.
    /// </summary>
    Task EliminarPermanentementeAsync(Guid supabaseUserId, CancellationToken cancellationToken);
}
