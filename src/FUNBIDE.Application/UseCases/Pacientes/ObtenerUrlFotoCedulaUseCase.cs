using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Pacientes;

public interface IObtenerUrlFotoCedulaUseCase : IUseCase<Guid, UrlFotoCedulaDto>
{
}

public sealed class ObtenerUrlFotoCedulaUseCase(
    IPacienteRepository pacienteRepository,
    ISupabaseStorageService storageService) : IObtenerUrlFotoCedulaUseCase
{
    public async Task<UrlFotoCedulaDto> EjecutarAsync(Guid pacienteId, CancellationToken cancellationToken)
    {
        var paciente = await pacienteRepository.ObtenerPorIdAsync(pacienteId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Paciente), pacienteId);

        if (paciente.FotoCedulaPath is null)
        {
            throw new RecursoNoEncontradoException("FotoCedula", pacienteId);
        }

        var url = await storageService.GenerarUrlFirmadaCedulaAsync(paciente.FotoCedulaPath, cancellationToken);

        return new UrlFotoCedulaDto(url);
    }
}
