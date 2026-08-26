using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Servicios;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Servicios;

public interface ICrearServicioUseCase : IUseCase<CrearServicioRequest, ServicioDto>
{
}

public sealed class CrearServicioUseCase(
    IServicioRepository servicioRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : ICrearServicioUseCase
{
    public async Task<ServicioDto> EjecutarAsync(CrearServicioRequest request, CancellationToken cancellationToken)
    {
        var existente = await servicioRepository.ObtenerPorCodigoAsync(request.Codigo, cancellationToken);
        if (existente is not null)
        {
            throw new CodigoServicioEnUsoException(request.Codigo);
        }

        var servicio = new Servicio(
            request.Codigo, request.Nombre, request.Precio1, request.Precio2, request.Precio3, request.Especialidad);

        await servicioRepository.AgregarAsync(servicio, cancellationToken);
        await servicioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "servicios.crear",
            recurso: $"servicios/{servicio.Id}",
            detalle: new { servicio.Codigo, servicio.Nombre, servicio.Precio1, servicio.Precio2, servicio.Precio3 },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 201,
            cancellationToken: cancellationToken);

        return new ServicioDto(
            servicio.Id, servicio.Codigo, servicio.Nombre, servicio.Precio1, servicio.Precio2, servicio.Precio3,
            servicio.Especialidad?.ToString(), servicio.Activo);
    }
}
