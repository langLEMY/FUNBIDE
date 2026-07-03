using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Domain.Entities;

public sealed class Paciente : Entity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;
    public DocumentoIdentidad Documento { get; private set; } = null!;
    public string? Telefono { get; private set; }

    /// <summary>
    /// Ruta dentro del bucket privado de Supabase Storage, no una URL pública: la foto
    /// de la cédula es un documento de identidad, más sensible que una foto de perfil
    /// de personal, así que solo se sirve vía URL firmada de corta duración
    /// (<see cref="Application.Common.Interfaces.ISupabaseStorageService.GenerarUrlFirmadaCedulaAsync"/>).
    /// </summary>
    public string? FotoCedulaPath { get; private set; }

    public string NombreCompleto => $"{Nombre} {Apellido}";

    private Paciente() { }

    public Paciente(string nombre, string apellido, DocumentoIdentidad documento, string? telefono)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        }

        if (string.IsNullOrWhiteSpace(apellido))
        {
            throw new ArgumentException("El apellido es obligatorio.", nameof(apellido));
        }

        Nombre = nombre.Trim();
        Apellido = apellido.Trim();
        Documento = documento;
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
    }

    public void ActualizarDatos(string nombre, string apellido, DocumentoIdentidad documento, string? telefono)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        }

        if (string.IsNullOrWhiteSpace(apellido))
        {
            throw new ArgumentException("El apellido es obligatorio.", nameof(apellido));
        }

        Nombre = nombre.Trim();
        Apellido = apellido.Trim();
        Documento = documento;
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
    }

    public void ActualizarFotoCedula(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("La ruta de la foto de cédula no puede estar vacía.", nameof(path));
        }

        FotoCedulaPath = path;
    }
}
