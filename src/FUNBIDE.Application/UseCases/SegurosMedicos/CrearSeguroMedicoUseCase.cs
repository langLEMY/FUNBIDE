using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.SegurosMedicos;

public interface ICrearSeguroMedicoUseCase : IUseCase<CrearSeguroMedicoRequest, SeguroMedicoDto>
{
}

public sealed class CrearSeguroMedicoUseCase(
    ISeguroMedicoRepository seguroMedicoRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : ICrearSeguroMedicoUseCase
{
    public async Task<SeguroMedicoDto> EjecutarAsync(CrearSeguroMedicoRequest request, CancellationToken cancellationToken)
    {
        var existente = await seguroMedicoRepository.ObtenerPorNombreAsync(request.Nombre, cancellationToken);
        if (existente is not null)
        {
            throw new NombreSeguroMedicoEnUsoException(request.Nombre);
        }

        var seguro = new SeguroMedico(request.Nombre, request.PorcentajeCobertura);

        await seguroMedicoRepository.AgregarAsync(seguro, cancellationToken);
        await seguroMedicoRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "seguros-medicos.crear",
            recurso: $"seguros-medicos/{seguro.Id}",
            detalle: new { seguro.Nombre, seguro.PorcentajeCobertura },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 201,
            cancellationToken: cancellationToken);

        return new SeguroMedicoDto(seguro.Id, seguro.Nombre, seguro.PorcentajeCobertura, seguro.Activo);
    }
}
