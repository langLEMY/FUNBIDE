using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Pacientes;

public interface IListarPacientesUseCase : IUseCase<IReadOnlyList<PacienteDto>>
{
}

public sealed class ListarPacientesUseCase(IPacienteRepository pacienteRepository) : IListarPacientesUseCase
{
    public async Task<IReadOnlyList<PacienteDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var pacientes = await pacienteRepository.ObtenerTodosAsync(cancellationToken);

        return pacientes
            .Select(p => new PacienteDto(
                p.Id, p.Nombre, p.Apellido, p.Documento.Valor, p.Telefono, p.FotoCedulaPath is not null))
            .ToList();
    }
}
