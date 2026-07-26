using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Interfaces;

public interface ICitaRepository
{
    Task<Cita?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Cita>> ObtenerPorDoctorYEstadoAsync(
        Guid doctorId, EstadoCita estado, CancellationToken cancellationToken);

    /// <summary>Ids distintos de pacientes que alguna vez tuvieron una cita con este doctor (cualquier estado) — para el Dashboard de Doctor, que no tiene un vínculo directo Paciente-Doctor en el dominio.</summary>
    Task<IReadOnlyList<Guid>> ObtenerPacienteIdsDistintosPorDoctorAsync(Guid doctorId, CancellationToken cancellationToken);

    Task<bool> ExisteAlgunaParaPacienteAsync(Guid pacienteId, CancellationToken cancellationToken);

    /// <summary>true si el paciente ya tiene una cita Programada o EnEspera con ese doctor (evita duplicar una llegada walk-in).</summary>
    Task<bool> TieneCitaActivaAsync(Guid pacienteId, Guid doctorId, CancellationToken cancellationToken);

    /// <summary>Para la Agenda de Recepción: todas las citas (cualquier doctor), filtrables por fecha y/o doctor.</summary>
    Task<IReadOnlyList<Cita>> ObtenerPorFiltroAsync(
        DateOnly? fecha, Guid? doctorId, CancellationToken cancellationToken);

    /// <summary>
    /// Sala de espera: citas en <see cref="EstadoCita.EnEspera"/> (sin importar la fecha:
    /// es un estado físico "está aquí ahora", no puede quedar de un día anterior en la
    /// práctica) más las <see cref="EstadoCita.Programada"/> cuyo <c>Intervalo.Inicio</c>
    /// cae en <paramref name="hoy"/> (para no listar como "esperando" una cita agendada
    /// para otro día).
    /// </summary>
    Task<IReadOnlyList<Cita>> ObtenerSalaDeEsperaAsync(DateOnly hoy, CancellationToken cancellationToken);

    /// <summary>
    /// Citas completadas que todavía no tienen un <c>Cobro</c> asociado ("pasa a caja").
    /// Sin acotar por fecha a propósito: una consulta completada hace días que sigue sin
    /// cobrarse es, si acaso, más urgente de mostrar, no menos.
    /// </summary>
    Task<IReadOnlyList<Cita>> ObtenerPendientesDeCobroAsync(CancellationToken cancellationToken);

    /// <summary>
    /// true si el doctor ya tiene una cita Programada o EnEspera que se solapa con el
    /// intervalo dado. <paramref name="excluirCitaId"/> permite comparar contra las demás
    /// citas del doctor al reprogramar una ya existente, sin que choque consigo misma.
    /// </summary>
    Task<bool> TieneChoqueDeHorarioAsync(
        Guid doctorId, DateTimeOffset inicio, DateTimeOffset fin, Guid? excluirCitaId, CancellationToken cancellationToken);

    Task AgregarAsync(Cita cita, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
