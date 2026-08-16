namespace FUNBIDE.Application.DTOs.Permisos;

public sealed record ActualizarPermisosDeUsuarioRequest(Guid UsuarioId, IReadOnlyList<PermisoDeseadoDto> Permisos);
