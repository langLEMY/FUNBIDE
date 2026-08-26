using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Servicios;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Servicios;

public interface IDesactivarServicioUseCase : IUseCase<Guid, ServicioDto>
{
}

public sealed class DesactivarServicioUseCase(
    IServicioRepository servicioRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IDesactivarServicioUseCase
{
    public async Task<ServicioDto> EjecutarAsync(Guid servicioId, CancellationToken cancellationToken)
    {
        var servicio = await servicioRepository.ObtenerPorIdAsync(servicioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Servicio), servicioId);

        servicio.Desactivar();
        await servicioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "servicios.desactivar",
            recurso: $"servicios/{servicio.Id}",
            detalle: new { servicio.Codigo, servicio.Nombre },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new ServicioDto(
            servicio.Id, servicio.Codigo, servicio.Nombre, servicio.Precio1, servicio.Precio2, servicio.Precio3,
            servicio.Especialidad?.ToString(), servicio.Activo);
    }
}
