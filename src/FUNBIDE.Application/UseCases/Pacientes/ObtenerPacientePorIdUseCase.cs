using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Pacientes;

public interface IObtenerPacientePorIdUseCase : IUseCase<Guid, PacienteDto>
{
}

/// <summary>
/// Un paciente puntual por id — necesario para páginas que abren un paciente directo
/// (p. ej. el historial clínico) sin depender de que esté entre los primeros resultados
/// de <see cref="IListarPacientesUseCase"/> (que pagina y no garantiza incluirlo).
/// </summary>
public sealed class ObtenerPacientePorIdUseCase(IPacienteRepository pacienteRepository) : IObtenerPacientePorIdUseCase
{
    public async Task<PacienteDto> EjecutarAsync(Guid id, CancellationToken cancellationToken)
    {
        var paciente = await pacienteRepository.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Paciente), id);

        return new PacienteDto(
            paciente.Id, paciente.Nombre, paciente.Apellido, paciente.Documento.Valor, paciente.Telefono,
            paciente.FotoCedulaPath is not null, paciente.Edad, paciente.Condicion, paciente.Estado, paciente.UltimaVisita);
    }
}
