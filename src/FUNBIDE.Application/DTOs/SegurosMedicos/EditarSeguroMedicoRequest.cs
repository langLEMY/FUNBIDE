namespace FUNBIDE.Application.DTOs.SegurosMedicos;

public sealed record EditarSeguroMedicoRequest(Guid SeguroMedicoId, string Nombre, decimal PorcentajeCobertura);
