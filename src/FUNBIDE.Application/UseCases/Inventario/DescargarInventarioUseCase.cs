using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Inventario;

public interface IDescargarInventarioUseCase : IUseCase<DescargarInventarioRequest, MovimientoInventarioDto>
{
}

/// <summary>
/// Descarga stock de farmacia de forma atómica: bloquea la fila del ítem, valida
/// la invariante de dominio y registra el movimiento dentro de la misma transacción.
/// Si <see cref="InventarioItem.DescargarStock"/> lanza por stock insuficiente, la
/// transacción completa se revierte y no queda ningún rastro parcial (ACID).
/// </summary>
public sealed class DescargarInventarioUseCase(
    IInventarioRepository inventarioRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IDescargarInventarioUseCase
{
    public Task<MovimientoInventarioDto> EjecutarAsync(
        DescargarInventarioRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            var item = await inventarioRepository.ObtenerConBloqueoAsync(request.InventarioItemId, ct)
                ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.InventarioItem), request.InventarioItemId);

            item.DescargarStock(request.Cantidad);

            var movimiento = new MovimientoInventario(
                item.Id, currentUser.UsuarioId, TipoMovimientoInventario.Descargo,
                request.Cantidad, item.StockActual, request.Referencia);

            await inventarioRepository.RegistrarMovimientoAsync(movimiento, ct);
            await inventarioRepository.GuardarCambiosAsync(ct);

            await auditoriaLogService.RegistrarEventoAsync(
                accion: "inventario.descargo",
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
