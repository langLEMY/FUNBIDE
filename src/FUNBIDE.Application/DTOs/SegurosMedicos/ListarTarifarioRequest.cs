using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.SegurosMedicos;

public sealed record ListarTarifarioRequest(Guid SeguroMedicoId, PlanSenasa Plan);
