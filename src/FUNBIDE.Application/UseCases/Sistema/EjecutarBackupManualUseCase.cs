using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sistema;

namespace FUNBIDE.Application.UseCases.Sistema;

public interface IEjecutarBackupManualUseCase : IUseCase<ResultadoBackupManualDto>
{
}

/// <summary>
/// Botón "Forzar backup ahora" (LEMY, Mi Perfil > Avanzado): dispara <see cref="IBackupEjecutorService"/>
/// fuera del ciclo periódico de <c>DatabaseBackupHostedService</c>, para no tener que esperar
/// hasta la próxima corrida automática antes de una operación riesgosa. Nunca deja escapar la
/// excepción: la traduce a un resultado con Exitoso=false para que el frontend la muestre.
/// </summary>
public sealed class EjecutarBackupManualUseCase(
    IBackupEjecutorService backupEjecutor,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : IEjecutarBackupManualUseCase
{
    public async Task<ResultadoBackupManualDto> EjecutarAsync(CancellationToken cancellationToken)
    {
        var ahora = dateTimeProvider.UtcNow;
        bool exitoso;
        string? mensaje = null;

        try
        {
            await backupEjecutor.EjecutarAsync(cancellationToken);
            exitoso = true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            exitoso = false;
            mensaje = ex.Message;
        }

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "sistema.backup-manual",
            recurso: "sistema/backup-manual",
            detalle: new { Exitoso = exitoso, Mensaje = mensaje },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: exitoso ? 200 : 500,
            cancellationToken: cancellationToken);

        return new ResultadoBackupManualDto(exitoso, ahora, mensaje);
    }
}
