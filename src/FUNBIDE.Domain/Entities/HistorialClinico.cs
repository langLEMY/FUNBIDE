using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Entrada inmutable del historial clínico de un paciente. Es append-only por diseño:
/// no expone ningún método de mutación una vez construida.
/// </summary>
public sealed class EntradaHistorialClinico : AppendOnlyEntity
{
    public Guid PacienteId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid? CitaId { get; private set; }
    public DocumentoJson Contenido { get; private set; } = null!;

    private EntradaHistorialClinico() { }

    public EntradaHistorialClinico(Guid pacienteId, Guid doctorId, DocumentoJson contenido, Guid? citaId = null)
    {
        PacienteId = pacienteId;
        DoctorId = doctorId;
        CitaId = citaId;
        Contenido = contenido;
    }
}
