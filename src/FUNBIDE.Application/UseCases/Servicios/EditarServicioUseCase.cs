using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Servicios;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Servicios;

public interface IEditarServicioUseCase : IUseCase<EditarServicioRequest, ServicioDto>
{
}

public sealed class EditarServicioUseCase(
    IServicioRepository servicioRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IEditarServicioUseCase
{
    public async Task<ServicioDto> EjecutarAsync(EditarServicioRequest request, CancellationToken cancellationToken)
    {
        var servicio = await servicioRepository.ObtenerPorIdAsync(request.ServicioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Servicio), request.ServicioId);

        servicio.ActualizarDatos(request.Nombre, request.Precio1, request.Precio2, request.Precio3, request.Especialidad);
        await servicioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "servicios.editar",
            recurso: $"servicios/{servicio.Id}",
            detalle: new { servicio.Codigo, servicio.Nombre, servicio.Precio1, servicio.Precio2, servicio.Precio3 },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new ServicioDto(
            servicio.Id, servicio.Codigo, servicio.Nombre, servicio.Precio1, servicio.Precio2, servicio.Precio3,
            servicio.Especialidad?.ToString(), servicio.Activo);
    }
}
