using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Interfaces;

public interface ITarifarioProcedimientoRepository
{
    Task<IReadOnlyList<TarifarioProcedimiento>> ObtenerPorSeguroYPlanAsync(
        Guid seguroMedicoId, PlanSenasa plan, CancellationToken cancellationToken);

    Task<TarifarioProcedimiento?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Para el import: todas las filas de ese seguro+plan, para reconciliar por nombre de procedimiento.</summary>
    Task<IReadOnlyList<TarifarioProcedimiento>> ObtenerParaImportarAsync(
        Guid seguroMedicoId, PlanSenasa plan, CancellationToken cancellationToken);

    Task AgregarAsync(TarifarioProcedimiento tarifario, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
