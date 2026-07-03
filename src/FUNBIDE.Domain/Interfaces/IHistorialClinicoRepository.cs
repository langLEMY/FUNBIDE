using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

/// <summary>
/// Repositorio deliberadamente sin métodos de actualización ni eliminación:
/// el historial clínico es append-only a nivel de contrato de dominio.
/// </summary>
public interface IHistorialClinicoRepository
{
    Task<IReadOnlyList<EntradaHistorialClinico>> ObtenerPorPacienteAsync(
        Guid pacienteId, CancellationToken cancellationToken);

    Task RegistrarAsync(EntradaHistorialClinico entrada, CancellationToken cancellationToken);
}
