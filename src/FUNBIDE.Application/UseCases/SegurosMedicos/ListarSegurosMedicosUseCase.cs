using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.SegurosMedicos;

public interface IListarSegurosMedicosUseCase : IUseCase<bool, IReadOnlyList<SeguroMedicoDto>>
{
}

/// <summary>Catálogo de ARS. Caja solo necesita las activas para el combo de Cobros; Admin/Lemy pueden pedir todas para administrarlas.</summary>
public sealed class ListarSegurosMedicosUseCase(ISeguroMedicoRepository seguroMedicoRepository) : IListarSegurosMedicosUseCase
{
    public async Task<IReadOnlyList<SeguroMedicoDto>> EjecutarAsync(bool incluirInactivos, CancellationToken cancellationToken)
    {
        var seguros = await seguroMedicoRepository.ObtenerTodosAsync(incluirInactivos, cancellationToken);

        return seguros.Select(s => new SeguroMedicoDto(s.Id, s.Nombre, s.PorcentajeCobertura, s.Activo)).ToList();
    }
}
