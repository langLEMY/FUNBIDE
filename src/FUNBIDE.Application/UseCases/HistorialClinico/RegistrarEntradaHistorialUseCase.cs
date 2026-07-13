using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.HistorialClinico;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Application.UseCases.HistorialClinico;

public interface IRegistrarEntradaHistorialUseCase : IUseCase<RegistrarEntradaHistorialRequest, EntradaHistorialDto>
{
}

/// <summary>
/// Único punto de escritura del historial clínico. No existe (ni existirá) un caso
/// de uso de actualización o borrado: el repositorio de dominio no expone esas
/// operaciones, así que intentarlo es un error de compilación, no solo de negocio.
/// </summary>
public sealed class RegistrarEntradaHistorialUseCase(
    IHistorialClinicoRepository historialRepository,
    IPacienteRepository pacienteRepository,
    ICitaRepository citaRepository,
    ICurrentUserService currentUser) : IRegistrarEntradaHistorialUseCase
{
    public async Task<EntradaHistorialDto> EjecutarAsync(
        RegistrarEntradaHistorialRequest request, CancellationToken cancellationToken)
    {
        // Sin FK en la tabla (ver EntradaHistorialClinicoConfiguration): validar acá es la
        // única defensa contra un pacienteId/citaId inventado, que dejaría un registro
        // huérfano permanente en un historial append-only (no hay caso de uso de borrado).
        _ = await pacienteRepository.ObtenerPorIdAsync(request.PacienteId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Paciente), request.PacienteId);

        if (request.CitaId.HasValue)
        {
            _ = await citaRepository.ObtenerPorIdAsync(request.CitaId.Value, cancellationToken)
                ?? throw new RecursoNoEncontradoException(nameof(Cita), request.CitaId.Value);
        }

        var contenido = DocumentoJson.Crear(request.Contenido.GetRawText());
        var entrada = new EntradaHistorialClinico(
            request.PacienteId, currentUser.UsuarioId, contenido, request.CitaId);

        await historialRepository.RegistrarAsync(entrada, cancellationToken);

        return new EntradaHistorialDto(
            entrada.Id, entrada.PacienteId, entrada.DoctorId, entrada.CitaId,
            entrada.Contenido.Valor, entrada.RegistradoEn);
    }
}
