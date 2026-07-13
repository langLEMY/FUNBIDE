using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface ISeguroMedicoRepository
{
    Task<IReadOnlyList<SeguroMedico>> ObtenerTodosAsync(bool incluirInactivos, CancellationToken cancellationToken);

    Task<SeguroMedico?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<SeguroMedico?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken);

    Task AgregarAsync(SeguroMedico seguro, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
