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
        // Trim antes de chequear: el constructor de InventarioItem recorta el código antes
        // de guardarlo, así que sin este mismo trim acá " MED-001" (con espacio) pasaba el
        // chequeo de duplicado y terminaba creando un segundo ítem con el código "MED-001".
        var codigo = request.Codigo.Trim();
        if (await inventarioRepository.ExisteCodigoAsync(codigo, cancellationToken))
        {
            throw new CodigoInventarioEnUsoException(codigo);
        }

        var item = new InventarioItem(
            codigo, request.Nombre, request.StockInicial, request.Categoria, request.StockMinimo);

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
