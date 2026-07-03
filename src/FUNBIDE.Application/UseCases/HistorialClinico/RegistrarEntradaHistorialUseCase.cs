using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.HistorialClinico;
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
    ICurrentUserService currentUser) : IRegistrarEntradaHistorialUseCase
{
    public async Task<EntradaHistorialDto> EjecutarAsync(
        RegistrarEntradaHistorialRequest request, CancellationToken cancellationToken)
    {
        var contenido = DocumentoJson.Crear(request.Contenido.GetRawText());
        var entrada = new EntradaHistorialClinico(
            request.PacienteId, currentUser.UsuarioId, contenido, request.CitaId);

        await historialRepository.RegistrarAsync(entrada, cancellationToken);

        return new EntradaHistorialDto(
            entrada.Id, entrada.PacienteId, entrada.DoctorId, entrada.CitaId,
            entrada.Contenido.Valor, entrada.RegistradoEn);
    }
}
