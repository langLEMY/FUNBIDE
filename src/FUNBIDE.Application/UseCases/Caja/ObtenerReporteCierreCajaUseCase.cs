using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Caja;

public interface IObtenerReporteCierreCajaUseCase : IUseCase<Guid, ReporteCierreCajaDto>
{
}

/// <summary>
/// Arma el detalle imprimible del arqueo de un turno: desglose por método de pago y cada
/// egreso/ingreso manual, para que la cajera (o Admin desde el historial) pueda dejar
/// constancia impresa de lo que pasó en el turno, no solo el total del cierre. Funciona
/// tanto para un turno ya cerrado como (en teoría) uno todavía abierto, aunque el flujo
/// normal del frontend solo lo pide justo después de <c>CerrarTurnoCajaUseCase</c>.
/// </summary>
public sealed class ObtenerReporteCierreCajaUseCase(
    ITurnoCajaRepository turnoCajaRepository,
    ICobroRepository cobroRepository,
    IMovimientoFinancieroRepository movimientoRepository,
    IUsuarioRepository usuarioRepository) : IObtenerReporteCierreCajaUseCase
{
    public async Task<ReporteCierreCajaDto> EjecutarAsync(Guid turnoId, CancellationToken cancellationToken)
    {
        var turno = await turnoCajaRepository.ObtenerPorIdAsync(turnoId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.TurnoCaja), turnoId);

        var cobros = await cobroRepository.ObtenerPorTurnoAsync(turno.Id, cancellationToken);
        var movimientos = await movimientoRepository.ObtenerPorTurnoAsync(turno.Id, cancellationToken);

        var idsUsuarios = new List<Guid> { turno.UsuarioAperturaId };
        if (turno.UsuarioCierreId.HasValue)
        {
            idsUsuarios.Add(turno.UsuarioCierreId.Value);
        }
        var nombresUsuarios = await usuarioRepository.ObtenerNombresPorIdsAsync(idsUsuarios, cancellationToken);

        // Por línea de pago (Cobro.Pagos), no por cobro entero — igual que
        // CerrarTurnoCajaUseCase: un cobro puede traer varios métodos a la vez.
        var totalesPorMetodo = cobros
            .SelectMany(c => c.Pagos)
            .GroupBy(p => p.Metodo)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Monto));

        var egresos = movimientos
            .Where(m => m.Tipo == TipoMovimientoFinanciero.Egreso)
            .OrderBy(m => m.RegistradoEn)
            .Select(m => new MovimientoReporteCierreDto(m.Concepto, m.Monto, m.RegistradoEn))
            .ToList();

        var ingresosManuales = movimientos
            .Where(m => m.Tipo == TipoMovimientoFinanciero.Ingreso)
            .OrderBy(m => m.RegistradoEn)
            .Select(m => new MovimientoReporteCierreDto(m.Concepto, m.Monto, m.RegistradoEn))
            .ToList();

        return new ReporteCierreCajaDto(
            turno.Id,
            nombresUsuarios.GetValueOrDefault(turno.UsuarioAperturaId, "—"),
            turno.UsuarioCierreId.HasValue ? nombresUsuarios.GetValueOrDefault(turno.UsuarioCierreId.Value) : null,
            turno.MontoInicial,
            turno.AbiertoEn,
            turno.CerradoEn,
            turno.MontoFinalContado ?? 0,
            turno.MontoEsperado ?? 0,
            turno.Diferencia ?? 0,
            turno.Notas,
            cobros.Count,
            cobros.Sum(c => c.MontoPagado),
            totalesPorMetodo,
            egresos,
            ingresosManuales);
    }
}
