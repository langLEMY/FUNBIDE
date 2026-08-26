using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.SegurosMedicos;

public interface IEditarTarifarioProcedimientoUseCase : IUseCase<EditarTarifarioProcedimientoRequest, TarifarioProcedimientoDto>
{
}

/// <summary>
/// Edita a mano una fila puntual del tarifario — sobre todo para darle a una aseguradora
/// (o corregirle) su <see cref="TarifarioProcedimiento.MontoFondo"/> sin tener que rehacer
/// un import completo del Excel del plan por un solo procedimiento. Mismo patrón que
/// <c>EditarServicioUseCase</c>: reescribe todos los montos de la fila, no un parche
/// parcial.
/// </summary>
public sealed class EditarTarifarioProcedimientoUseCase(
    ITarifarioProcedimientoRepository tarifarioRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IEditarTarifarioProcedimientoUseCase
{
    public async Task<TarifarioProcedimientoDto> EjecutarAsync(
        EditarTarifarioProcedimientoRequest request, CancellationToken cancellationToken)
    {
        var tarifario = await tarifarioRepository.ObtenerPorIdAsync(request.TarifarioProcedimientoId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(TarifarioProcedimiento), request.TarifarioProcedimientoId);

        tarifario.ActualizarMontos(request.MontoSeguro, request.MontoPaciente, request.MontoTotal, request.MontoFondo);
        tarifario.AsignarEspecialidad(request.Especialidad);
        await tarifarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "tarifario-procedimientos.editar",
            recurso: $"tarifario-procedimientos/{tarifario.Id}",
            detalle: new
            {
                tarifario.Procedimiento,
                tarifario.MontoSeguro,
                tarifario.MontoPaciente,
                tarifario.MontoTotal,
                tarifario.MontoFondo,
            },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new TarifarioProcedimientoDto(
            tarifario.Id, tarifario.SeguroMedicoId, tarifario.Plan.ToString(), tarifario.Procedimiento,
            tarifario.MontoSeguro, tarifario.MontoPaciente, tarifario.MontoTotal, tarifario.MontoFondo,
            tarifario.Especialidad?.ToString());
    }
}
