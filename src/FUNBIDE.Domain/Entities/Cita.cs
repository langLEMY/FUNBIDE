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

    /// <summary>
    /// El paciente llegó físicamente y pasa a la sala de espera: desde una cita agendada
    /// (<see cref="EstadoCita.Programada"/>) o desde un walk-in recién creado sin agendar
    /// (<see cref="EstadoCita.Pendiente"/>), que es como recepción registra una llegada
    /// directa sin cita previa.
    /// </summary>
    public void RegistrarLlegada()
    {
        if (Estado != EstadoCita.Pendiente && Estado != EstadoCita.Programada)
        {
            throw new InvalidOperationException(
                $"Solo una cita pendiente o programada puede pasar a en espera. Estado actual: {Estado}.");
        }

        Estado = EstadoCita.EnEspera;
    }

    /// <summary>
    /// Acepta completarse desde <see cref="EstadoCita.Programada"/> o
    /// <see cref="EstadoCita.EnEspera"/>: el paso por recepción (<see cref="RegistrarLlegada"/>)
    /// es un enriquecimiento del flujo, no un requisito — un doctor debe poder seguir
    /// completando una cita que nunca pasó por el check-in de recepción.
    /// </summary>
    public void Completar(string notasCierre)
    {
        if (Estado != EstadoCita.Programada && Estado != EstadoCita.EnEspera)
        {
            throw new InvalidOperationException(
                $"Solo una cita programada o en espera puede completarse. Estado actual: {Estado}.");
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
