using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Pacientes;

public sealed record ListarPacientesRequest(int Pagina, int TamanoPagina, string? Busqueda, EstadoPaciente? Estado);
