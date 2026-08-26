using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Servicios;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Servicios;

public interface IListarServiciosUseCase : IUseCase<bool, IReadOnlyList<ServicioDto>>
{
}

/// <summary>Catálogo de precios privados. Fondos lo necesita como picklist de motivo en Agenda/Recepción y como selector de Cobros; Admin/Lemy pueden pedir todos para administrarlo.</summary>
public sealed class ListarServiciosUseCase(IServicioRepository servicioRepository) : IListarServiciosUseCase
{
    public async Task<IReadOnlyList<ServicioDto>> EjecutarAsync(bool incluirInactivos, CancellationToken cancellationToken)
    {
        var servicios = await servicioRepository.ObtenerTodosAsync(incluirInactivos, cancellationToken);

        return servicios
            .Select(s => new ServicioDto(s.Id, s.Codigo, s.Nombre, s.Precio1, s.Precio2, s.Precio3, s.Especialidad?.ToString(), s.Activo))
            .ToList();
    }
}
