using FUNBIDE.Domain.Common;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Catálogo de aseguradoras (ARS) que Caja puede seleccionar al procesar un cobro. Se
/// desactiva en vez de borrarse para no perder la referencia de los <see cref="Cobro"/>
/// ya registrados con su id.
/// </summary>
public sealed class SeguroMedico : Entity
{
    public string Nombre { get; private set; } = string.Empty;
    /// <summary>
    /// Vestigial: era el % de cobertura usado para calcular la cobertura de un cobro
    /// cuando no había tarifario cargado. Ese cálculo automático quedó desactivado (ver
    /// RegistrarCobroRequestValidator — hoy todo cobro con seguro exige un procedimiento
    /// del tarifario) — nullable y sin pedirse más al crear/editar una aseguradora nueva;
    /// se conserva solo por las aseguradoras que ya lo tenían cargado.
    /// </summary>
    public decimal? PorcentajeCobertura { get; private set; }
    public bool Activo { get; private set; } = true;

    private SeguroMedico() { }

    public SeguroMedico(string nombre, decimal? porcentajeCobertura = null)
    {
        ValidarDatos(nombre, porcentajeCobertura);

        Nombre = nombre.Trim();
        PorcentajeCobertura = porcentajeCobertura;
    }

    public void ActualizarDatos(string nombre, decimal? porcentajeCobertura)
    {
        ValidarDatos(nombre, porcentajeCobertura);

        Nombre = nombre.Trim();
        PorcentajeCobertura = porcentajeCobertura;
    }

    public void Desactivar() => Activo = false;

    public void Reactivar() => Activo = true;

    private static void ValidarDatos(string nombre, decimal? porcentajeCobertura)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de la aseguradora es obligatorio.", nameof(nombre));
        }

        if (porcentajeCobertura is <= 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(porcentajeCobertura), "El porcentaje de cobertura debe estar entre 0 y 100.");
        }
    }
}
