using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Citas;

/// <summary>
/// Pagina/TamanoPagina son opcionales: sin ellos (caso de hoy en AgendaPage.tsx),
/// ListarAgendaUseCase sigue devolviendo todo lo que matchea Fecha/DoctorId/Estado de una
/// sola vez, igual que siempre — ver el comentario en ICitaRepository.ObtenerPaginadoAsync.
/// </summary>
public sealed record ListarAgendaRequest(
    DateOnly? Fecha, Guid? DoctorId, EstadoCita? Estado = null, int? Pagina = null, int? TamanoPagina = null);
