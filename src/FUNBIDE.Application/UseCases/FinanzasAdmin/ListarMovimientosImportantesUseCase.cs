using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.FinanzasAdmin;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.FinanzasAdmin;

public interface IListarMovimientosImportantesUseCase : IUseCase<ListarMovimientosImportantesRequest, IReadOnlyList<MovimientoImportanteDto>>
{
}

/// <summary>
/// Junta <see cref="Domain.Entities.Cobro"/> (ingresos por pacientes) y
/// <see cref="Domain.Entities.MovimientoFinanciero"/> (ingresos/egresos manuales, incluidos
/// los gastos registrados por Admin) en una sola lista cronológica, para que Admin vea
/// "todos los movimientos importantes" en un solo lugar sin tener que reconstruirlos
/// indirectamente desde el log de auditoría.
/// </summary>
public sealed class ListarMovimientosImportantesUseCase(
    ICobroRepository cobroRepository,
    IMovimientoFinancieroRepository movimientoRepository,
    IPacienteRepository pacienteRepository) : IListarMovimientosImportantesUseCase
{
    public async Task<IReadOnlyList<MovimientoImportanteDto>> EjecutarAsync(
        ListarMovimientosImportantesRequest request, CancellationToken cancellationToken)
    {
        var cobros = await cobroRepository.ObtenerPorRangoAsync(request.Desde, request.Hasta, cancellationToken);
        var movimientos = await movimientoRepository.ObtenerPorRangoAsync(request.Desde, request.Hasta, cancellationToken);

        var nombresPacientes = await pacienteRepository.ObtenerNombresPorIdsAsync(
            cobros.Select(c => c.PacienteId).Distinct().ToList(), cancellationToken);

        var resultado = new List<MovimientoImportanteDto>(cobros.Count + movimientos.Count);

        resultado.AddRange(cobros.Select(c => new MovimientoImportanteDto(
            c.Id,
            c.RegistradoEn,
            "Cobro",
            TipoMovimientoFinanciero.Ingreso,
            c.Concepto,
            c.MontoPagado,
            nombresPacientes.GetValueOrDefault(c.PacienteId, "—"))));

        resultado.AddRange(movimientos.Select(m => new MovimientoImportanteDto(
            m.Id, m.RegistradoEn, "Manual", m.Tipo, m.Concepto, m.Monto, null)));

        return resultado.OrderByDescending(m => m.Fecha).ToList();
    }
}
