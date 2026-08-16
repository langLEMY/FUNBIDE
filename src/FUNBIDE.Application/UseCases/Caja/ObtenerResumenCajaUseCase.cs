using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Caja;

public interface IObtenerResumenCajaUseCase : IUseCase<ResumenCajaDto>
{
}

/// <summary>
/// Balance del dashboard de Caja. Los conteos informativos (sala de espera, deuda,
/// pendientes de cobro) se calculan siempre; los montos de dinero solo tienen sentido
/// mientras hay un turno abierto — con la caja cerrada se devuelven en cero.
/// </summary>
public sealed class ObtenerResumenCajaUseCase(
    ITurnoCajaRepository turnoCajaRepository,
    ICobroRepository cobroRepository,
    IMovimientoFinancieroRepository movimientoRepository,
    ICitaRepository citaRepository,
    IDateTimeProvider dateTimeProvider) : IObtenerResumenCajaUseCase
{
    public async Task<ResumenCajaDto> EjecutarAsync(CancellationToken cancellationToken)
    {
        var turno = await turnoCajaRepository.ObtenerAbiertoAsync(cancellationToken);
        var ahoraLocal = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, dateTimeProvider.ZonaHorariaClinica);
        var hoy = DateOnly.FromDateTime(ahoraLocal.DateTime);
        var salaDeEspera = await citaRepository.ObtenerSalaDeEsperaAsync(hoy, cancellationToken);
        var pendientesDeCobro = await citaRepository.ObtenerPendientesDeCobroAsync(cancellationToken);
        var pacientesConDeuda = await cobroRepository.ContarPacientesConDeudaAsync(cancellationToken);

        if (turno is null)
        {
            return new ResumenCajaDto(
                CajaAbierta: false,
                EfectivoEnCaja: 0,
                TotalFacturadoHoy: 0,
                TotalEfectivo: 0,
                TotalTarjeta: 0,
                TotalTransferencia: 0,
                TotalCubiertoPorSeguro: 0,
                SalidasAutorizadas: 0,
                PacientesEnEspera: salaDeEspera.Count,
                PacientesConDeudaPendiente: pacientesConDeuda,
                ConsultasPendientesDeCobro: pendientesDeCobro.Count,
                GastosAdminEnElTurno: 0);
        }

        var cobros = await cobroRepository.ObtenerPorTurnoAsync(turno.Id, cancellationToken);
        var movimientos = await movimientoRepository.ObtenerPorTurnoAsync(turno.Id, cancellationToken);

        // Se suma por línea de pago (Cobro.Pagos), no por cobro entero: un mismo cobro
        // puede traer varias líneas con métodos distintos (ver "dividir el pago").
        var pagos = cobros.SelectMany(c => c.Pagos).ToList();
        var totalEfectivo = pagos.Where(p => p.Metodo == MetodoPago.Efectivo).Sum(p => p.Monto);
        var totalTarjeta = pagos.Where(p => p.Metodo == MetodoPago.Tarjeta).Sum(p => p.Monto);
        var totalTransferencia = pagos.Where(p => p.Metodo == MetodoPago.Transferencia).Sum(p => p.Monto);
        var totalCubiertoPorSeguro = cobros.Sum(c => c.MontoCobertura ?? 0);
        var salidasAutorizadas = movimientos.Where(m => m.Tipo == TipoMovimientoFinanciero.Egreso).Sum(m => m.Monto);
        var ingresosManuales = movimientos.Where(m => m.Tipo == TipoMovimientoFinanciero.Ingreso).Sum(m => m.Monto);

        // Gastos que Admin registró (sin TurnoCajaId, ver RegistrarGastoAdminUseCase)
        // mientras este turno estuvo abierto: no entran en "movimientos" (filtrados por
        // turno) ni afectan el efectivo de caja, pero Caja necesita verlos para no
        // duplicar el mismo gasto en "salida autorizada".
        var movimientosEnVentana = await movimientoRepository.ObtenerPorRangoAsync(turno.AbiertoEn, dateTimeProvider.UtcNow, cancellationToken);
        var gastosAdminEnElTurno = movimientosEnVentana
            .Where(m => m.TurnoCajaId is null && m.Tipo == TipoMovimientoFinanciero.Egreso)
            .Sum(m => m.Monto);

        return new ResumenCajaDto(
            CajaAbierta: true,
            EfectivoEnCaja: turno.MontoInicial + totalEfectivo + ingresosManuales - salidasAutorizadas,
            TotalFacturadoHoy: cobros.Sum(c => c.MontoTotal),
            TotalEfectivo: totalEfectivo,
            TotalTarjeta: totalTarjeta,
            TotalTransferencia: totalTransferencia,
            TotalCubiertoPorSeguro: totalCubiertoPorSeguro,
            SalidasAutorizadas: salidasAutorizadas,
            PacientesEnEspera: salaDeEspera.Count,
            PacientesConDeudaPendiente: pacientesConDeuda,
            ConsultasPendientesDeCobro: pendientesDeCobro.Count,
            GastosAdminEnElTurno: gastosAdminEnElTurno);
    }
}
