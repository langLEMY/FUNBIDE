using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface ICobroRepository
{
    Task<IReadOnlyList<Cobro>> ObtenerPorTurnoAsync(Guid turnoCajaId, CancellationToken cancellationToken);

    /// <summary>Para el panel de Finanzas de Admin: todos los cobros entre dos fechas (de cualquier turno).</summary>
    Task<IReadOnlyList<Cobro>> ObtenerPorRangoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, CancellationToken cancellationToken);

    /// <summary>Suma de <see cref="Cobro.MontoPendiente"/> de todos los cobros del paciente, histórico.</summary>
    Task<decimal> ObtenerDeudaTotalPorPacienteAsync(Guid pacienteId, CancellationToken cancellationToken);

    /// <summary>Igual que <see cref="ObtenerDeudaTotalPorPacienteAsync"/> pero en lote, para no hacer N consultas al enriquecer una lista de citas.</summary>
    Task<IReadOnlyDictionary<Guid, decimal>> ObtenerDeudaTotalPorPacientesAsync(
        IReadOnlyCollection<Guid> pacienteIds, CancellationToken cancellationToken);

    /// <summary>Cuenta pacientes distintos con al menos un cobro con saldo pendiente, para el aviso del dashboard.</summary>
    Task<int> ContarPacientesConDeudaAsync(CancellationToken cancellationToken);

    /// <summary>true si ya existe un cobro registrado contra esta cita (para no cobrarla dos veces).</summary>
    Task<bool> ExisteCobroParaCitaAsync(Guid citaId, CancellationToken cancellationToken);

    Task AgregarAsync(Cobro cobro, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
