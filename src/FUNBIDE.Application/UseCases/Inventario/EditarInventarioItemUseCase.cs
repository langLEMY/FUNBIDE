using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Inventario;

public interface IEditarInventarioItemUseCase : IUseCase<EditarInventarioItemRequest, InventarioItemDto>
{
}

/// <summary>
/// Edita el nombre/stock mínimo de un ítem y corrige su stock a un valor exacto
/// (p. ej. tras un conteo físico). Igual que el descargo, bloquea la fila dentro
/// de una transacción para no pisar un despacho concurrente.
/// </summary>
public sealed class EditarInventarioItemUseCase(
    IInventarioRepository inventarioRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IEditarInventarioItemUseCase
{
    public Task<InventarioItemDto> EjecutarAsync(EditarInventarioItemRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            var item = await inventarioRepository.ObtenerConBloqueoAsync(request.InventarioItemId, ct)
                ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.InventarioItem), request.InventarioItemId);

            item.ActualizarDatos(request.Nombre, request.StockMinimo);
            item.AjustarStock(request.StockActual);

            await inventarioRepository.GuardarCambiosAsync(ct);

            await auditoriaLogService.RegistrarEventoAsync(
                accion: "inventario.edicion",
                recurso: $"inventario/{item.Id}",
                detalle: new { item.Codigo, item.Nombre, item.StockActual, item.StockMinimo },
                usuarioId: currentUser.UsuarioId,
                codigoRespuestaHttp: 200,
                cancellationToken: ct);

            return new InventarioItemDto(item.Id, item.Codigo, item.Nombre, item.StockActual, item.Categoria, item.StockMinimo);
        }, cancellationToken);
    }
}
