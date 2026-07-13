namespace FUNBIDE.Application.DTOs.SegurosMedicos;

public sealed record SeguroMedicoDto(Guid Id, string Nombre, decimal PorcentajeCobertura, bool Activo);
