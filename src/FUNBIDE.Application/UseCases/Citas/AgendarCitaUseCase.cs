using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IAgendarCitaUseCase : IUseCase<AgendarCitaRequest, CitaDto>
{
}

/// <summary>
/// "Cita rápida" de Recepción: crea y programa la cita en un solo paso (a diferencia del
/// flujo del Doctor, que primero crea una <see cref="Domain.Enums.EstadoCita.Pendiente"/>
/// y la programa después), validando que el doctor no tenga ya otra cita en ese horario.
/// </summary>
public sealed class AgendarCitaUseCase(
    ICitaRepository citaRepository,
    IPacienteRepository pacienteRepository,
    IUsuarioRepository usuarioRepository) : IAgendarCitaUseCase
{
    public async Task<CitaDto> EjecutarAsync(AgendarCitaRequest request, CancellationToken cancellationToken)
    {
        _ = await pacienteRepository.ObtenerPorIdAsync(request.PacienteId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Paciente), request.PacienteId);

        var doctor = await usuarioRepository.ObtenerPorSupabaseUserIdAsync(request.DoctorId, cancellationToken);
        if (doctor is null || doctor.Rol != RolUsuario.Doctor)
        {
            throw new RecursoNoEncontradoException(nameof(Usuario), request.DoctorId);
        }

        var hayChoque = await citaRepository.TieneChoqueDeHorarioAsync(
            request.DoctorId, request.Inicio, request.Fin, excluirCitaId: null, cancellationToken);
        if (hayChoque)
        {
            throw new HorarioNoDisponibleException(request.DoctorId, request.Inicio, request.Fin);
        }

        var cita = new Cita(request.PacienteId, request.DoctorId, request.Motivo);
        cita.Programar(IntervaloCita.Crear(request.Inicio, request.Fin));

        await citaRepository.AgregarAsync(cita, cancellationToken);
        await citaRepository.GuardarCambiosAsync(cancellationToken);

        return new CitaDto(
            cita.Id, cita.PacienteId, cita.DoctorId, cita.Motivo, cita.Estado,
            cita.Intervalo!.Inicio, cita.Intervalo.Fin, cita.NotasCierre);
    }
}
