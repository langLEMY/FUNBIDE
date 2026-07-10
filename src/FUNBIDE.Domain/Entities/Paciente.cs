using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Domain.Entities;

public sealed class Paciente : Entity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;
    public DocumentoIdentidad Documento { get; private set; } = null!;
    public string? Telefono { get; private set; }
    public int? Edad { get; private set; }
    public string? Condicion { get; private set; }
    public EstadoPaciente Estado { get; private set; } = EstadoPaciente.Activo;
    public DateOnly? UltimaVisita { get; private set; }

    /// <summary>
    /// Ruta dentro del bucket privado de Supabase Storage, no una URL pública: la foto
    /// de la cédula es un documento de identidad, más sensible que una foto de perfil
    /// de personal, así que solo se sirve vía URL firmada de corta duración
    /// (<see cref="Application.Common.Interfaces.ISupabaseStorageService.GenerarUrlFirmadaCedulaAsync"/>).
    /// </summary>
    public string? FotoCedulaPath { get; private set; }

    public string NombreCompleto => $"{Nombre} {Apellido}";

    private Paciente() { }

    public Paciente(string nombre, string apellido, DocumentoIdentidad documento, string? telefono, int? edad = null, string? condicion = null)
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
        Edad = edad;
        Condicion = string.IsNullOrWhiteSpace(condicion) ? null : condicion.Trim();
        Estado = EstadoPaciente.Activo;
    }

    public void ActualizarDatos(
        string nombre, string apellido, DocumentoIdentidad documento, string? telefono, int? edad, string? condicion)
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
        Edad = edad;
        Condicion = string.IsNullOrWhiteSpace(condicion) ? null : condicion.Trim();
    }

    /// <summary>
    /// Completa con datos de una reimportación solo los campos que estén vacíos, sin
    /// sobrescribir los que el paciente ya tuviera cargados (a diferencia de
    /// <see cref="ActualizarDatos"/>, pensado para una edición manual completa).
    /// </summary>
    public void CompletarDesdeImportacion(string? telefono, int? edad, string? condicion)
    {
        Telefono ??= string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        Edad ??= edad;
        Condicion ??= string.IsNullOrWhiteSpace(condicion) ? null : condicion.Trim();
    }

    public void CambiarEstado(EstadoPaciente estado)
    {
        Estado = estado;
    }

    /// <summary>Se invoca cuando una cita de este paciente se marca como completada.</summary>
    public void RegistrarVisita(DateOnly fecha)
    {
        UltimaVisita = fecha;
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
