namespace FUNBIDE.Infrastructure.Security;

/// <summary>
/// Contraseña hasheada para el modo de autenticación local (Auth:Provider=Local).
/// Vive fuera del modelo de dominio a propósito: es un detalle de implementación de
/// la infraestructura de autenticación, igual que "auth.users" de Supabase no es
/// parte del dominio de FUNBIDE. La clave es <c>Usuario.SupabaseUserId</c>.
/// </summary>
public sealed class CredencialLocal
{
    public Guid UsuarioId { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;

    private CredencialLocal()
    {
    }

    public CredencialLocal(Guid usuarioId, string passwordHash)
    {
        UsuarioId = usuarioId;
        PasswordHash = passwordHash;
    }

    public void ActualizarPasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
