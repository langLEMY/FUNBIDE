using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.SegurosMedicos;

public interface IListarSegurosMedicosUseCase : IUseCase<bool, IReadOnlyList<SeguroMedicoDto>>
{
}

/// <summary>Catálogo de ARS. Caja solo necesita las activas para el combo de Cobros; Admin/Lemy pueden pedir todas para administrarlas.</summary>
public sealed class ListarSegurosMedicosUseCase(
    ISeguroMedicoRepository seguroMedicoRepository,
    ITarifarioProcedimientoRepository tarifarioRepository) : IListarSegurosMedicosUseCase
{
    public async Task<IReadOnlyList<SeguroMedicoDto>> EjecutarAsync(bool incluirInactivos, CancellationToken cancellationToken)
    {
        var seguros = await seguroMedicoRepository.ObtenerTodosAsync(incluirInactivos, cancellationToken);

        // Reemplaza el viejo hardcode "nombre contiene SENASA" del frontend para decidir si
        // mostrar el selector de tarifario — ahora cualquier aseguradora con filas activas
        // en TarifarioProcedimiento (Senasa, Renacer, Aps...) lo activa por sí sola.
        var seguroIdsConTarifario = await tarifarioRepository.ObtenerSeguroIdsConTarifarioActivoAsync(cancellationToken);

        return seguros
            .Select(s => new SeguroMedicoDto(s.Id, s.Nombre, s.PorcentajeCobertura, s.Activo, seguroIdsConTarifario.Contains(s.Id)))
            .ToList();
    }
}
