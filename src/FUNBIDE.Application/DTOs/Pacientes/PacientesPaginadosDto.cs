namespace FUNBIDE.Application.DTOs.Pacientes;

public sealed record PacientesPaginadosDto(
    IReadOnlyList<PacienteDto> Items, int Total, int Pagina, int TamanoPagina);
