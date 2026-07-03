using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Cita médica asignada a un doctor. Controla sus propias transiciones de estado
/// para impedir que la capa de aplicación deje la cita en un estado inconsistente.
/// </summary>
public sealed class Cita : Entity
{
    public Guid PacienteId { get; private set; }
    public Guid DoctorId { get; private set; }
    public IntervaloCita? Intervalo { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public EstadoCita Estado { get; private set; } = EstadoCita.Pendiente;
    public string? NotasCierre { get; private set; }

    private Cita() { }

    public Cita(Guid pacienteId, Guid doctorId, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("El motivo de la cita es obligatorio.", nameof(motivo));
        }

        PacienteId = pacienteId;
        DoctorId = doctorId;
        Motivo = motivo.Trim();
        Estado = EstadoCita.Pendiente;
    }

    public void Programar(IntervaloCita intervalo)
    {
        if (Estado != EstadoCita.Pendiente)
        {
            throw new InvalidOperationException($"Solo una cita pendiente puede programarse. Estado actual: {Estado}.");
        }

        Intervalo = intervalo;
        Estado = EstadoCita.Programada;
    }

    public void Completar(string notasCierre)
    {
        if (Estado != EstadoCita.Programada)
        {
            throw new InvalidOperationException($"Solo una cita programada puede completarse. Estado actual: {Estado}.");
        }

        NotasCierre = notasCierre;
        Estado = EstadoCita.Completada;
    }

    public void Cancelar()
    {
        if (Estado == EstadoCita.Completada)
        {
            throw new InvalidOperationException("No es posible cancelar una cita ya completada.");
        }

        Estado = EstadoCita.Cancelada;
    }
}
