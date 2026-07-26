namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Abstrae la subida de fotos de perfil del personal a un almacenamiento de objetos
/// (Supabase Storage). El caso de uso no conoce buckets ni URLs firmadas: solo entrega
/// el contenido y recibe la URL pública final para guardarla en <c>Usuario.FotoPerfilUrl</c>.
/// </summary>
public interface ISupabaseStorageService
{
    Task<string> SubirFotoPerfilAsync(
        Guid usuarioId, Stream contenido, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Sube la foto de la cédula a un bucket privado y devuelve la ruta interna
    /// (bucket/objeto), no una URL — el bucket no es público. Guardar esa ruta y pedir
    /// <see cref="GenerarUrlFirmadaCedulaAsync"/> cada vez que se necesite mostrarla.
    /// </summary>
    Task<string> SubirFotoCedulaAsync(
        Guid pacienteId, Stream contenido, string contentType, CancellationToken cancellationToken);

    /// <summary>Genera una URL firmada de corta duración para ver una foto de cédula privada.</summary>
    Task<string> GenerarUrlFirmadaCedulaAsync(string rutaObjeto, CancellationToken cancellationToken);

    /// <summary>
    /// Chequeo de conectividad real (no una constante) para el panel "Estado del sistema" /
    /// "Probar conexión" de LEMY — sin ejecutar ninguna subida de negocio.
    /// </summary>
    Task<bool> VerificarConexionAsync(CancellationToken cancellationToken);
}
