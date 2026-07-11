namespace FUNBIDE.Infrastructure.Security;

public enum ProveedorAutenticacion
{
    Supabase,
    Local,
}

/// <summary>
/// Elige el proveedor de autenticación de toda la app: "Supabase" (nube, producción,
/// valor por defecto) o "Local" (JWT emitido por esta misma API sin ninguna llamada
/// de red — usado por la copia offline/USB de demo). Ver <see cref="DependencyInjection"/>.
/// </summary>
public sealed class AuthOptions
{
    public const string SeccionConfiguracion = "Auth";

    public ProveedorAutenticacion Provider { get; init; } = ProveedorAutenticacion.Supabase;
}
