using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Interfaces;

public interface ITarifarioProcedimientoRepository
{
    Task<IReadOnlyList<TarifarioProcedimiento>> ObtenerPorSeguroYPlanAsync(
        Guid seguroMedicoId, PlanAseguradora plan, CancellationToken cancellationToken);

    Task<TarifarioProcedimiento?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Para el import: todas las filas de ese seguro+plan, para reconciliar por nombre de procedimiento.</summary>
    Task<IReadOnlyList<TarifarioProcedimiento>> ObtenerParaImportarAsync(
        Guid seguroMedicoId, PlanAseguradora plan, CancellationToken cancellationToken);

    /// <summary>Ids de aseguradoras con al menos una fila de tarifario activa (para saber si ofrecen tarifario por procedimiento).</summary>
    Task<IReadOnlySet<Guid>> ObtenerSeguroIdsConTarifarioActivoAsync(CancellationToken cancellationToken);

    Task AgregarAsync(TarifarioProcedimiento tarifario, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
