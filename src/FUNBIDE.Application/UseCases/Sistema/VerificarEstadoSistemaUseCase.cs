using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sistema;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Sistema;

public interface IVerificarEstadoSistemaUseCase : IUseCase<EstadoSistemaDto>
{
}

/// <summary>
/// Alimenta el panel "Estado del sistema" (LEMY y Admin). Reporta conectividad a la base
/// de datos, al almacenamiento de archivos, si el backup automático dejó rastro (ver
/// <c>DatabaseBackupHostedService</c>) cuándo fue su última corrida y si tuvo éxito, y si
/// el modo mantenimiento está activo.
/// </summary>
public sealed class VerificarEstadoSistemaUseCase(
    IEstadoBaseDeDatosService estadoBaseDeDatos,
    IEstadoBackupService estadoBackup,
    ISupabaseStorageService almacenamiento,
    IConfiguracionSistemaRepository configuracionRepository) : IVerificarEstadoSistemaUseCase
{
    public async Task<EstadoSistemaDto> EjecutarAsync(CancellationToken cancellationToken)
    {
        var operativa = await estadoBaseDeDatos.VerificarConexionAsync(cancellationToken);
        var backup = await estadoBackup.ObtenerUltimoEstadoAsync(cancellationToken);
        var almacenamientoOperativo = await almacenamiento.VerificarConexionAsync(cancellationToken);
        var configuracion = await configuracionRepository.ObtenerAsync(cancellationToken);

        return new EstadoSistemaDto(
            operativa,
            backup?.UltimaEjecucionUtc,
            backup?.Exitoso,
            almacenamientoOperativo,
            configuracion?.ModoMantenimientoActivo ?? false,
            configuracion?.ModoMantenimientoMensaje);
    }
}
