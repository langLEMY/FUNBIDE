using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Interfaces;

public interface ICitaRepository
{
    Task<Cita?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Cita>> ObtenerPorDoctorYEstadoAsync(
        Guid doctorId, EstadoCita estado, CancellationToken cancellationToken);

    Task<bool> ExisteAlgunaParaPacienteAsync(Guid pacienteId, CancellationToken cancellationToken);

    Task AgregarAsync(Cita cita, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
