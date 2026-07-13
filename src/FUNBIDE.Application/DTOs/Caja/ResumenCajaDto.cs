namespace FUNBIDE.Application.DTOs.Caja;

/// <summary>
/// Balance del turno de caja actual. Cuando no hay un turno abierto
/// (<see cref="CajaAbierta"/> es false) todos los montos quedan en cero: el dashboard
/// debe mostrar el estado "caja cerrada" en vez de cifras del turno anterior.
/// </summary>
public sealed record ResumenCajaDto(
    bool CajaAbierta,
    decimal EfectivoEnCaja,
    decimal TotalFacturadoHoy,
    decimal TotalEfectivo,
    decimal TotalTarjeta,
    decimal TotalTransferencia,
    decimal TotalCubiertoPorSeguro,
    decimal SalidasAutorizadas,
    int PacientesEnEspera,
    int PacientesConDeudaPendiente,
    int ConsultasPendientesDeCobro,
    /// <summary>
    /// Gastos que Admin registró desde Finanzas (sin turno asociado) mientras este turno
    /// estuvo abierto — Caja no los ve en su propio listado (filtrado por TurnoCajaId), así
    /// que sin este aviso un cajero podría no saber que ya se registraron y duplicarlos.
    /// </summary>
    decimal GastosAdminEnElTurno);
