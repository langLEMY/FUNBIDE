using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IRegistrarLlegadaSinCitaUseCase : IUseCase<RegistrarLlegadaSinCitaRequest, CitaDto>
{
}

/// <summary>Llegada rápida de un paciente sin cita previa (walk-in): crea la cita y la pasa directo a la sala de espera.</summary>
public sealed class RegistrarLlegadaSinCitaUseCase(
    ICitaRepository citaRepository,
    IPacienteRepository pacienteRepository,
    IUsuarioRepository usuarioRepository) : IRegistrarLlegadaSinCitaUseCase
{
    public async Task<CitaDto> EjecutarAsync(RegistrarLlegadaSinCitaRequest request, CancellationToken cancellationToken)
    {
        _ = await pacienteRepository.ObtenerPorIdAsync(request.PacienteId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Paciente), request.PacienteId);

        var doctor = await usuarioRepository.ObtenerPorSupabaseUserIdAsync(request.DoctorId, cancellationToken);
        if (doctor is null || doctor.Rol != RolUsuario.Doctor)
        {
            throw new RecursoNoEncontradoException(nameof(Usuario), request.DoctorId);
        }

        var yaTieneCitaActiva = await citaRepository.TieneCitaActivaAsync(request.PacienteId, request.DoctorId, cancellationToken);
        if (yaTieneCitaActiva)
        {
            throw new PacienteConCitaActivaException(request.PacienteId, request.DoctorId);
        }

        var cita = new Cita(request.PacienteId, request.DoctorId, request.Motivo);
        cita.RegistrarLlegada();

        await citaRepository.AgregarAsync(cita, cancellationToken);
        await citaRepository.GuardarCambiosAsync(cancellationToken);

        return new CitaDto(cita.Id, cita.PacienteId, cita.DoctorId, cita.Motivo, cita.Estado, null, null, null);
    }
}
