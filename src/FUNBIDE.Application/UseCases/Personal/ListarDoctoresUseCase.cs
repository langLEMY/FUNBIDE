using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface IListarDoctoresUseCase : IUseCase<IReadOnlyList<DoctorSimpleDto>>
{
}

/// <summary>Solo id + nombre: alimenta los selectores de doctor de Agenda/Recepción sin exponer el listado completo de personal.</summary>
public sealed class ListarDoctoresUseCase(IUsuarioRepository usuarioRepository) : IListarDoctoresUseCase
{
    public async Task<IReadOnlyList<DoctorSimpleDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var doctores = await usuarioRepository.ObtenerActivosPorRolAsync(RolUsuario.Doctor, cancellationToken);

        // SupabaseUserId, no el Id local: es lo que Agenda/Recepción mandan como
        // Cita.DoctorId, y el resto del sistema (ObtenerCitasPorEstadoUseCase,
        // ProgramarCitaUseCase, etc.) compara DoctorId contra currentUser.UsuarioId,
        // que también es el SupabaseUserId (claim "sub" del JWT).
        return doctores.Select(d => new DoctorSimpleDto(d.SupabaseUserId, d.NombreCompleto, d.Especialidad)).ToList();
    }
}
