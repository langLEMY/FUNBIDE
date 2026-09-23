using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Inventario;

public interface IRegistrarEntradaInventarioUseCase : IUseCase<RegistrarEntradaInventarioRequest, MovimientoInventarioDto>
{
}

/// <summary>
/// Registra una entrada de stock (compra, donación recibida, devolución) de forma atómica —
/// mismo patrón que <see cref="DescargarInventarioUseCase"/> pero sumando en vez de restar.
/// Junto con el descargo, es ahora el único camino para mover <see cref="InventarioItem.StockActual"/>:
/// "Editar" ya no permite escribir un número de stock a mano (ver <c>EditarInventarioItemUseCase</c>),
/// así que todo movimiento de stock queda en el historial de <see cref="MovimientoInventario"/>.
/// </summary>
public sealed class RegistrarEntradaInventarioUseCase(
    IInventarioRepository inventarioRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IRegistrarEntradaInventarioUseCase
{
    public Task<MovimientoInventarioDto> EjecutarAsync(
        RegistrarEntradaInventarioRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            var item = await inventarioRepository.ObtenerConBloqueoAsync(request.InventarioItemId, ct)
                ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.InventarioItem), request.InventarioItemId);

            item.Reingresar(request.Cantidad);

            var movimiento = new MovimientoInventario(
                item.Id, currentUser.UsuarioId, TipoMovimientoInventario.Reingreso,
                request.Cantidad, item.StockActual, request.Referencia);

            await inventarioRepository.RegistrarMovimientoAsync(movimiento, ct);
            await inventarioRepository.GuardarCambiosAsync(ct);

            await auditoriaLogService.RegistrarEventoAsync(
                accion: "inventario.entrada",
                recurso: $"inventario/{item.Id}",
                detalle: new { item.Codigo, request.Cantidad, item.StockActual },
                usuarioId: currentUser.UsuarioId,
                codigoRespuestaHttp: 200,
                cancellationToken: ct);

            return new MovimientoInventarioDto(
                movimiento.Id, item.Id, item.Codigo, movimiento.Cantidad,
                movimiento.StockResultante, movimiento.RegistradoEn);
        }, cancellationToken);
    }
}
