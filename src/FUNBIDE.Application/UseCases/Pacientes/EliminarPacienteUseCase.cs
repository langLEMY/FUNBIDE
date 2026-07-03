using FUNBIDE.Application.Common;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Pacientes;

public interface IEliminarPacienteUseCase : IUseCase<Guid, Guid>
{
}

/// <summary>
/// Bloquea el borrado si el paciente ya tiene citas o entradas de historial clínico:
/// eliminarlo dejaría esos registros huérfanos, violando el carácter append-only del historial.
/// </summary>
public sealed class EliminarPacienteUseCase(
    IPacienteRepository pacienteRepository,
    ICitaRepository citaRepository,
    IHistorialClinicoRepository historialClinicoRepository) : IEliminarPacienteUseCase
{
    public async Task<Guid> EjecutarAsync(Guid pacienteId, CancellationToken cancellationToken)
    {
        var paciente = await pacienteRepository.ObtenerPorIdAsync(pacienteId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Paciente), pacienteId);

        if (await citaRepository.ExisteAlgunaParaPacienteAsync(pacienteId, cancellationToken))
        {
            throw new OperacionNoPermitidaException(
                "No se puede eliminar el paciente: tiene citas registradas. Elimina primero las citas asociadas.");
        }

        var historial = await historialClinicoRepository.ObtenerPorPacienteAsync(pacienteId, cancellationToken);
        if (historial.Count > 0)
        {
            throw new OperacionNoPermitidaException(
                "No se puede eliminar el paciente: tiene historial clínico registrado, el cual no puede eliminarse.");
        }

        pacienteRepository.Eliminar(paciente);
        await pacienteRepository.GuardarCambiosAsync(cancellationToken);

        return pacienteId;
    }
}
