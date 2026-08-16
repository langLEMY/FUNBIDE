using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Permisos;

/// <summary>Estado de un módulo para un usuario puntual: efectivo, y si es un override o el default de su rol.</summary>
public sealed record ModuloPermisoEstadoDto(ModuloPermiso Modulo, bool Concedido, bool EsOverride, bool DefaultDelRol);
