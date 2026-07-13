using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.SegurosMedicos;

public interface IEditarSeguroMedicoUseCase : IUseCase<EditarSeguroMedicoRequest, SeguroMedicoDto>
{
}

public sealed class EditarSeguroMedicoUseCase(
    ISeguroMedicoRepository seguroMedicoRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IEditarSeguroMedicoUseCase
{
    public async Task<SeguroMedicoDto> EjecutarAsync(EditarSeguroMedicoRequest request, CancellationToken cancellationToken)
    {
        var seguro = await seguroMedicoRepository.ObtenerPorIdAsync(request.SeguroMedicoId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(SeguroMedico), request.SeguroMedicoId);

        if (!string.Equals(seguro.Nombre, request.Nombre.Trim(), StringComparison.Ordinal))
        {
            var otroConEseNombre = await seguroMedicoRepository.ObtenerPorNombreAsync(request.Nombre, cancellationToken);
            if (otroConEseNombre is not null && otroConEseNombre.Id != seguro.Id)
            {
                throw new NombreSeguroMedicoEnUsoException(request.Nombre);
            }
        }

        seguro.ActualizarDatos(request.Nombre, request.PorcentajeCobertura);
        await seguroMedicoRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "seguros-medicos.editar",
            recurso: $"seguros-medicos/{seguro.Id}",
            detalle: new { seguro.Nombre, seguro.PorcentajeCobertura },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new SeguroMedicoDto(seguro.Id, seguro.Nombre, seguro.PorcentajeCobertura, seguro.Activo);
    }
}
