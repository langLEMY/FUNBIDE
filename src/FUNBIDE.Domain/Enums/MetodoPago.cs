namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Cómo se cobró la porción a cargo del paciente. "Seguro" no es un método de pago: la
/// cobertura de la ARS es un bucket aparte (<see cref="Entities.Cobro.MontoCobertura"/>),
/// nunca dinero que entra físicamente a la caja.
/// </summary>
public enum MetodoPago
{
    Efectivo,
    Tarjeta,
    Transferencia
}
