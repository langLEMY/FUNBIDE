using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface ICrearCitaUseCase : IUseCase<CrearCitaRequest, CitaDto>
{
}

public sealed class CrearCitaUseCase(
    ICitaRepository citaRepository,
    ICurrentUserService currentUser) : ICrearCitaUseCase
{
    public async Task<CitaDto> EjecutarAsync(CrearCitaRequest request, CancellationToken cancellationToken)
    {
        var cita = new Cita(request.PacienteId, currentUser.UsuarioId, request.Motivo);

        await citaRepository.AgregarAsync(cita, cancellationToken);
        await citaRepository.GuardarCambiosAsync(cancellationToken);

        return new CitaDto(cita.Id, cita.PacienteId, cita.DoctorId, cita.Motivo, cita.Estado, null, null, null);
    }
}
