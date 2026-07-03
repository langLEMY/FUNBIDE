using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IPacienteRepository
{
    Task<IReadOnlyList<Paciente>> ObtenerTodosAsync(CancellationToken cancellationToken);

    Task<Paciente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Paciente?> ObtenerPorDocumentoAsync(string documento, CancellationToken cancellationToken);

    Task AgregarAsync(Paciente paciente, CancellationToken cancellationToken);

    void Eliminar(Paciente paciente);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
