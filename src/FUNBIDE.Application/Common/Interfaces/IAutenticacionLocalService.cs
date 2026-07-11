namespace FUNBIDE.Application.Common.Interfaces;

public sealed record TokenLocalResultado(string AccessToken, DateTimeOffset ExpiraEn);

/// <summary>
/// Verifica credenciales y emite un JWT en el modo de autenticación local
/// (Auth:Provider=Local). Solo se registra en DI cuando ese modo está activo.
/// </summary>
public interface IAutenticacionLocalService
{
    /// <summary>Devuelve null si el correo no existe, la contraseña no coincide, o el usuario está inactivo/eliminado.</summary>
    Task<TokenLocalResultado?> IniciarSesionAsync(
        string correo, string contrasena, CancellationToken cancellationToken);
}
