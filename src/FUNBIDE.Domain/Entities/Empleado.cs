using FUNBIDE.Domain.Common;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Directorio simple del personal del hospital: nombre y cargo, sin cuenta ni acceso
/// al sistema. Deliberadamente independiente de <see cref="Usuario"/> (que sí tiene
/// login vía Supabase Auth y un rol de negocio) — hay personal del hospital que nunca
/// necesita iniciar sesión en FUNBIDE pero igual se quiere tener registrado. Mutable
/// y con borrado real: no es append-only ni tiene relevancia de auditoría clínica.
/// </summary>
public sealed class Empleado : Entity
{
    public string NombreCompleto { get; private set; } = string.Empty;
    public string? Cargo { get; private set; }

    private Empleado() { }

    public Empleado(string nombreCompleto, string? cargo)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            throw new ArgumentException("El nombre completo es obligatorio.", nameof(nombreCompleto));
        }

        NombreCompleto = nombreCompleto.Trim();
        Cargo = string.IsNullOrWhiteSpace(cargo) ? null : cargo.Trim();
    }

    public void ActualizarDatos(string nombreCompleto, string? cargo)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            throw new ArgumentException("El nombre completo es obligatorio.", nameof(nombreCompleto));
        }

        NombreCompleto = nombreCompleto.Trim();
        Cargo = string.IsNullOrWhiteSpace(cargo) ? null : cargo.Trim();
    }
}
