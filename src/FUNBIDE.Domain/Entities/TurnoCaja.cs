using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Turno de caja: desde que alguien abre la caja con un monto inicial en efectivo hasta
/// que la cierra con el arqueo final. Mientras está <see cref="EstadoTurnoCaja.Abierto"/>,
/// los cobros y egresos de <see cref="Cobro"/>/<see cref="MovimientoFinanciero"/> quedan
/// ligados a este turno para poder reconciliarlos al cierre; una vez
/// <see cref="EstadoTurnoCaja.Cerrado"/> no se puede reabrir (hay que abrir uno nuevo).
/// </summary>
public sealed class TurnoCaja : Entity
{
    public Guid UsuarioAperturaId { get; private set; }
    public decimal MontoInicial { get; private set; }
    public DateTimeOffset AbiertoEn { get; private set; }
    public EstadoTurnoCaja Estado { get; private set; }
    public Guid? UsuarioCierreId { get; private set; }
    public decimal? MontoFinalContado { get; private set; }
    public decimal? MontoEsperado { get; private set; }

    /// <summary>MontoFinalContado - MontoEsperado. Positivo = sobrante, negativo = faltante.</summary>
    public decimal? Diferencia { get; private set; }
    public string? Notas { get; private set; }
    public DateTimeOffset? CerradoEn { get; private set; }

    private TurnoCaja() { }

    public TurnoCaja(Guid usuarioAperturaId, decimal montoInicial, DateTimeOffset abiertoEn)
    {
        if (montoInicial < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(montoInicial), "El monto inicial no puede ser negativo.");
        }

        UsuarioAperturaId = usuarioAperturaId;
        MontoInicial = montoInicial;
        AbiertoEn = abiertoEn;
        Estado = EstadoTurnoCaja.Abierto;
    }

    /// <summary>
    /// Cierra el turno con el arqueo: <paramref name="montoEsperado"/> es el efectivo que
    /// debería haber en caja (monto inicial + cobros en efectivo + ingresos manuales -
    /// egresos, igual fórmula que <c>ObtenerResumenCajaUseCase</c>) calculado por quien
    /// invoca, para dejarlo congelado junto con la diferencia en el momento del cierre —
    /// si se recalculara después, cobros/movimientos posteriores lo desfasarían.
    /// </summary>
    public void Cerrar(
        Guid usuarioCierreId, decimal montoFinalContado, decimal montoEsperado, string? notas, DateTimeOffset cerradoEn)
    {
        if (Estado != EstadoTurnoCaja.Abierto)
        {
            throw new InvalidOperationException("Solo un turno abierto puede cerrarse.");
        }

        if (montoFinalContado < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(montoFinalContado), "El monto contado no puede ser negativo.");
        }

        UsuarioCierreId = usuarioCierreId;
        MontoFinalContado = montoFinalContado;
        MontoEsperado = montoEsperado;
        Diferencia = montoFinalContado - montoEsperado;
        Notas = string.IsNullOrWhiteSpace(notas) ? null : notas.Trim();
        CerradoEn = cerradoEn;
        Estado = EstadoTurnoCaja.Cerrado;
    }
}
