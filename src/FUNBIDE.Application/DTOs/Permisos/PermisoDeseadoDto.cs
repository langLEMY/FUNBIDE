using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Permisos;

public sealed record PermisoDeseadoDto(ModuloPermiso Modulo, bool Concedido);
