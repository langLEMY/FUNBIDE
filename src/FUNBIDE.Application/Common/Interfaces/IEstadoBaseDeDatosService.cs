namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Chequeo de conectividad contra la base de datos, sin ejecutar ninguna consulta de
/// negocio — lo usa el panel de "Estado del sistema" de LEMY para distinguir un
/// problema real de infraestructura de cualquier otro tipo de error.
/// </summary>
public interface IEstadoBaseDeDatosService
{
    Task<bool> VerificarConexionAsync(CancellationToken cancellationToken);
}
