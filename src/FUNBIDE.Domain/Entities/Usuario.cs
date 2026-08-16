using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Proyección local del usuario autenticado en Supabase Auth. La fuente de verdad de
/// credenciales vive en Supabase; esta entidad solo enlaza el <see cref="SupabaseUserId"/>
/// con el rol y los datos de negocio propios de FUNBIDE.
/// </summary>
public sealed class Usuario : Entity
{
    public Guid SupabaseUserId { get; private set; }
    public string NombreCompleto { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;

    /// <summary>
    /// Identificador con el que la persona inicia sesión (en vez del correo). Único,
    /// normalizado a minúsculas. El correo se conserva aparte porque sigue siendo la
    /// identidad real en Supabase Auth (recuperación de contraseña, etc.) — ver
    /// <c>IResolverCorreoPorNombreUsuarioUseCase</c>, que traduce uno al otro antes de
    /// que el frontend llegue a llamar a Supabase.
    /// </summary>
    public string NombreUsuario { get; private set; } = string.Empty;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; } = true;
    public string? FotoPerfilUrl { get; private set; }

    /// <summary>Solo tiene sentido cuando <see cref="Rol"/> es <see cref="RolUsuario.Doctor"/> — ver <see cref="AsignarEspecialidad"/>.</summary>
    public EspecialidadMedica? Especialidad { get; private set; }

    /// <summary>
    /// true cuando a este usuario se le borró el acceso de Supabase Auth de forma
    /// irreversible (<c>EliminarUsuarioPermanentementeUseCase</c>), a diferencia de
    /// <see cref="Desactivar"/> (reversible con <see cref="Reactivar"/>). La fila local
    /// sigue existiendo por trazabilidad histórica, pero se excluye del listado de
    /// personal — ver <c>IUsuarioRepository.ObtenerTodosAsync</c>.
    /// </summary>
    public bool EliminadoPermanentemente { get; private set; }

    private Usuario() { }

    public Usuario(
        Guid supabaseUserId, string nombreCompleto, string correo, string nombreUsuario,
        RolUsuario rol, EspecialidadMedica? especialidad = null)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            throw new ArgumentException("El nombre completo es obligatorio.", nameof(nombreCompleto));
        }

        if (string.IsNullOrWhiteSpace(correo))
        {
            throw new ArgumentException("El correo es obligatorio.", nameof(correo));
        }

        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
        }

        SupabaseUserId = supabaseUserId;
        NombreCompleto = nombreCompleto.Trim();
        Correo = correo.Trim().ToLowerInvariant();
        NombreUsuario = nombreUsuario.Trim().ToLowerInvariant();
        Rol = rol;
        AsignarEspecialidad(especialidad);
    }

    /// <summary>
    /// Reasigna el rol de negocio de este usuario. Solo actualiza la proyección local:
    /// quien invoque este método (<c>CambiarRolUsuarioUseCase</c>) es responsable de
    /// sincronizar primero <c>app_metadata.role</c> en Supabase Auth, que es la fuente
    /// real que se proyecta como claim en el JWT y decide la autorización.
    /// </summary>
    /// <summary>
    /// Si el nuevo rol no es Doctor, limpia <see cref="Especialidad"/>: ya no aplica y
    /// dejarla puesta violaría la regla validada en <see cref="AsignarEspecialidad"/>.
    /// </summary>
    public void CambiarRol(RolUsuario nuevoRol)
    {
        Rol = nuevoRol;
        if (nuevoRol != RolUsuario.Doctor)
        {
            Especialidad = null;
        }
    }

    /// <summary>Solo un usuario con rol Doctor puede tener una especialidad médica asignada.</summary>
    public void AsignarEspecialidad(EspecialidadMedica? especialidad)
    {
        if (especialidad is not null && Rol != RolUsuario.Doctor)
        {
            throw new ArgumentException("Solo un usuario con rol Doctor puede tener una especialidad médica.", nameof(especialidad));
        }

        Especialidad = especialidad;
    }

    /// <summary>
    /// Actualiza nombre, correo y nombre de usuario. Igual que <see cref="CambiarRol"/>,
    /// quien invoque este método (<c>EditarUsuarioUseCase</c>) es responsable de
    /// sincronizar primero el correo en Supabase Auth si cambió, porque ahí vive la
    /// credencial de inicio de sesión.
    /// </summary>
    public void ActualizarDatos(string nombreCompleto, string correo, string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            throw new ArgumentException("El nombre completo es obligatorio.", nameof(nombreCompleto));
        }

        if (string.IsNullOrWhiteSpace(correo))
        {
            throw new ArgumentException("El correo es obligatorio.", nameof(correo));
        }

        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(nombreUsuario));
        }

        NombreCompleto = nombreCompleto.Trim();
        Correo = correo.Trim().ToLowerInvariant();
        NombreUsuario = nombreUsuario.Trim().ToLowerInvariant();
    }

    public void ActualizarFotoPerfil(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("La URL de la foto de perfil no puede estar vacía.", nameof(url));
        }

        FotoPerfilUrl = url;
    }

    public void Desactivar() => Activo = false;

    public void Reactivar() => Activo = true;

    public void EliminarPermanentemente()
    {
        Activo = false;
        EliminadoPermanentemente = true;
    }
}
