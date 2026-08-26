using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Catálogo de precios privados (pago particular, sin seguro médico) — equivalente al
/// tarifario por aseguradora (ver <see cref="TarifarioProcedimiento"/>) pero para pacientes
/// que pagan de su bolsillo. <see cref="Precio1"/>/<see cref="Precio2"/>/<see cref="Precio3"/>
/// se guardan tal cual vienen del Excel de origen (tres tarifas paralelas del sistema
/// contable anterior) sin asumir qué representa cada una. Se desactiva en vez de borrarse
/// para no perder la referencia de <see cref="Cobro"/> ya registrados con su código.
/// </summary>
public sealed class Servicio : Entity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio1 { get; private set; }
    public decimal Precio2 { get; private set; }
    public decimal Precio3 { get; private set; }

    /// <summary>
    /// Especialidad de FUNBIDE a la que corresponde este servicio, solo para agrupar y
    /// filtrar en la UI (selección encadenada especialidad → servicio → doctor) — no es
    /// una validación de negocio. Null cuando el servicio no tiene una especialidad clara
    /// (p. ej. la mayoría de los ítems importados del catálogo contable anterior).
    /// </summary>
    public EspecialidadMedica? Especialidad { get; private set; }

    public bool Activo { get; private set; } = true;

    private Servicio() { }

    public Servicio(
        string codigo, string nombre, decimal precio1, decimal precio2, decimal precio3,
        EspecialidadMedica? especialidad = null)
    {
        ValidarDatos(codigo, nombre, precio1, precio2, precio3);

        Codigo = codigo.Trim();
        Nombre = nombre.Trim();
        Precio1 = precio1;
        Precio2 = precio2;
        Precio3 = precio3;
        Especialidad = especialidad;
    }

    public void ActualizarDatos(string nombre, decimal precio1, decimal precio2, decimal precio3, EspecialidadMedica? especialidad)
    {
        ValidarDatos(Codigo, nombre, precio1, precio2, precio3);

        Nombre = nombre.Trim();
        Precio1 = precio1;
        Precio2 = precio2;
        Precio3 = precio3;
        Especialidad = especialidad;
    }

    /// <summary>Fija el stock/precio tras un reimport (ver <c>ImportarServiciosUseCase</c>) sin tocar la especialidad ya asignada a mano.</summary>
    public void ActualizarPrecios(string nombre, decimal precio1, decimal precio2, decimal precio3)
    {
        ValidarDatos(Codigo, nombre, precio1, precio2, precio3);

        Nombre = nombre.Trim();
        Precio1 = precio1;
        Precio2 = precio2;
        Precio3 = precio3;
    }

    public void Desactivar() => Activo = false;

    public void Reactivar() => Activo = true;

    private static void ValidarDatos(string codigo, string nombre, decimal precio1, decimal precio2, decimal precio3)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código del servicio es obligatorio.", nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del servicio es obligatorio.", nameof(nombre));
        }

        if (precio1 < 0 || precio2 < 0 || precio3 < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precio1), "Los precios no pueden ser negativos.");
        }
    }
}
