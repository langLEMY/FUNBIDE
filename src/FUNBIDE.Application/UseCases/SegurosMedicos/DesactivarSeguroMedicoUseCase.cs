using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.SegurosMedicos;

public interface IDesactivarSeguroMedicoUseCase : IUseCase<Guid, SeguroMedicoDto>
{
}

public sealed class DesactivarSeguroMedicoUseCase(
    ISeguroMedicoRepository seguroMedicoRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : IDesactivarSeguroMedicoUseCase
{
    public async Task<SeguroMedicoDto> EjecutarAsync(Guid seguroMedicoId, CancellationToken cancellationToken)
    {
        var seguro = await seguroMedicoRepository.ObtenerPorIdAsync(seguroMedicoId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(SeguroMedico), seguroMedicoId);

        seguro.Desactivar();
        await seguroMedicoRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "seguros-medicos.desactivar",
            recurso: $"seguros-medicos/{seguro.Id}",
            detalle: new { seguro.Nombre },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new SeguroMedicoDto(seguro.Id, seguro.Nombre, seguro.PorcentajeCobertura, seguro.Activo);
    }
}
