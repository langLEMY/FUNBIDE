using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Inventario;

public interface ICrearInventarioItemUseCase : IUseCase<CrearInventarioItemRequest, InventarioItemDto>
{
}

public sealed class CrearInventarioItemUseCase(
    IInventarioRepository inventarioRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : ICrearInventarioItemUseCase
{
    public async Task<InventarioItemDto> EjecutarAsync(
        CrearInventarioItemRequest request, CancellationToken cancellationToken)
    {
        if (await inventarioRepository.ExisteCodigoAsync(request.Codigo, cancellationToken))
        {
            throw new CodigoInventarioEnUsoException(request.Codigo);
        }

        var item = new InventarioItem(
            request.Codigo, request.Nombre, request.StockInicial, request.Categoria, request.StockMinimo);

        await inventarioRepository.AgregarAsync(item, cancellationToken);
        await inventarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "inventario.creacion",
            recurso: $"inventario/{item.Id}",
            detalle: new { item.Codigo, item.Nombre, item.Categoria, item.StockActual, item.StockMinimo },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 201,
            cancellationToken: cancellationToken);

        return new InventarioItemDto(item.Id, item.Codigo, item.Nombre, item.StockActual, item.Categoria, item.StockMinimo);
    }
}
