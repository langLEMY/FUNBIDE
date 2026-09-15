namespace FUNBIDE.Launcher.Actualizacion;

/// <summary>
/// Lo que hace falta para ofrecer y aplicar una actualización: a qué versión, de dónde
/// bajar el instalador, y de dónde bajar el hash SHA256 para verificarlo antes de
/// ejecutar nada (ver ServicioActualizacion.DescargarInstaladorAsync).
/// </summary>
public sealed record InfoActualizacion(
    string VersionTag,
    Uri UrlDescargaInstalador,
    Uri UrlDescargaHash,
    string NombreArchivo);
