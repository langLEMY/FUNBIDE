using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Pacientes;

public interface IActualizarFotoCedulaUseCase : IUseCase<ActualizarFotoCedulaRequest, PacienteDto>
{
}

public sealed class ActualizarFotoCedulaUseCase(
    IPacienteRepository pacienteRepository,
    ISupabaseStorageService storageService) : IActualizarFotoCedulaUseCase
{
    public async Task<PacienteDto> EjecutarAsync(ActualizarFotoCedulaRequest request, CancellationToken cancellationToken)
    {
        var paciente = await pacienteRepository.ObtenerPorIdAsync(request.PacienteId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Paciente), request.PacienteId);

        var ruta = await storageService.SubirFotoCedulaAsync(
            paciente.Id, request.Contenido, request.ContentType, cancellationToken);
        paciente.ActualizarFotoCedula(ruta);

        await pacienteRepository.GuardarCambiosAsync(cancellationToken);

        return new PacienteDto(
            paciente.Id, paciente.Nombre, paciente.Apellido, paciente.Documento.Valor, paciente.Telefono, paciente.FotoCedulaPath is not null);
    }
}
