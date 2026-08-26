using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.SegurosMedicos;

public interface ICrearTarifarioProcedimientoUseCase : IUseCase<CrearTarifarioProcedimientoRequest, TarifarioProcedimientoDto>
{
}

/// <summary>
/// Da de alta un procedimiento puntual del tarifario a mano, sin pasar por el import de
/// Excel — pensado para una aseguradora nueva que arranca con pocos procedimientos
/// negociados. El índice único (SeguroMedicoId, Plan, Procedimiento) es quien rechaza un
/// duplicado (ver <c>ExceptionHandlingMiddleware</c>, que ya traduce esa violación a 409):
/// no hace falta un chequeo previo.
/// </summary>
public sealed class CrearTarifarioProcedimientoUseCase(
    ITarifarioProcedimientoRepository tarifarioRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : ICrearTarifarioProcedimientoUseCase
{
    public async Task<TarifarioProcedimientoDto> EjecutarAsync(
        CrearTarifarioProcedimientoRequest request, CancellationToken cancellationToken)
    {
        var tarifario = new TarifarioProcedimiento(
            request.SeguroMedicoId, request.Plan, request.Procedimiento,
            request.MontoSeguro, request.MontoPaciente, request.MontoTotal,
            request.MontoFondo, request.Especialidad);

        await tarifarioRepository.AgregarAsync(tarifario, cancellationToken);
        await tarifarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "tarifario-procedimientos.crear",
            recurso: $"tarifario-procedimientos/{tarifario.Id}",
            detalle: new
            {
                tarifario.SeguroMedicoId,
                Plan = tarifario.Plan.ToString(),
                tarifario.Procedimiento,
                tarifario.MontoSeguro,
                tarifario.MontoPaciente,
                tarifario.MontoTotal,
                tarifario.MontoFondo,
            },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 201,
            cancellationToken: cancellationToken);

        return new TarifarioProcedimientoDto(
            tarifario.Id, tarifario.SeguroMedicoId, tarifario.Plan.ToString(), tarifario.Procedimiento,
            tarifario.MontoSeguro, tarifario.MontoPaciente, tarifario.MontoTotal, tarifario.MontoFondo,
            tarifario.Especialidad?.ToString());
    }
}
