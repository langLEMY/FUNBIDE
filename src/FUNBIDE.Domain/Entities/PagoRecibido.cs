using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Una línea de pago dentro de un <see cref="Cobro"/>: cuánto de lo cobrado entró por
/// ese método puntual. Un cobro puede tener varias (ej. una parte con tarjeta, otra en
/// efectivo) — así es como FUNBIDE sabe cuánto dinero entró realmente por cada método,
/// en vez de asumir que todo un cobro usa un único método. Sin identidad propia (owned
/// type de EF: vive y muere con su <see cref="Cobro"/>), la clave natural es
/// (CobroId, Metodo) — no tiene sentido que un mismo cobro traiga dos líneas del mismo
/// método, se consolidan en una sola al construir el <see cref="Cobro"/>.
/// </summary>
public sealed class PagoRecibido
{
    public MetodoPago Metodo { get; private set; }
    public decimal Monto { get; private set; }

    private PagoRecibido() { }

    public PagoRecibido(MetodoPago metodo, decimal monto)
    {
        if (monto <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monto), "El monto de cada pago debe ser mayor que cero.");
        }

        Metodo = metodo;
        Monto = monto;
    }
}
