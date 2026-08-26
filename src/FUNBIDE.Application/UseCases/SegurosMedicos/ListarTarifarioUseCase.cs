using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.SegurosMedicos;

public interface IListarTarifarioUseCase : IUseCase<ListarTarifarioRequest, IReadOnlyList<TarifarioProcedimientoDto>>
{
}

/// <summary>Alimenta el buscador de procedimientos de Cobros y el listado de Aseguradoras.</summary>
public sealed class ListarTarifarioUseCase(ITarifarioProcedimientoRepository tarifarioRepository) : IListarTarifarioUseCase
{
    public async Task<IReadOnlyList<TarifarioProcedimientoDto>> EjecutarAsync(
        ListarTarifarioRequest request, CancellationToken cancellationToken)
    {
        var filas = await tarifarioRepository.ObtenerPorSeguroYPlanAsync(request.SeguroMedicoId, request.Plan, cancellationToken);

        return filas
            .Select(t => new TarifarioProcedimientoDto(
                t.Id, t.SeguroMedicoId, t.Plan.ToString(), t.Procedimiento, t.MontoSeguro, t.MontoPaciente, t.MontoTotal,
                t.MontoFondo, t.Especialidad?.ToString()))
            .ToList();
    }
}
