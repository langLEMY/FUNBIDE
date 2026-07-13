using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Pacientes;

public interface IListarPacientesUseCase : IUseCase<ListarPacientesRequest, PacientesPaginadosDto>
{
}

public sealed class ListarPacientesUseCase(IPacienteRepository pacienteRepository) : IListarPacientesUseCase
{
    private const int TamanoPaginaPorDefecto = 50;
    private const int TamanoPaginaMaximo = 100;

    public async Task<PacientesPaginadosDto> EjecutarAsync(
        ListarPacientesRequest request, CancellationToken cancellationToken)
    {
        // Cota superior además del "< 1": sin esto, un pagina absurdamente grande puede
        // desbordar el int de (pagina - 1) * tamanoPagina más adelante (aritmética
        // unchecked por defecto en C#), produciendo un Skip negativo y un 500 en vez de
        // simplemente devolver una página vacía.
        var pagina = Math.Clamp(request.Pagina, 1, 1_000_000);
        var tamanoPagina = request.TamanoPagina < 1
            ? TamanoPaginaPorDefecto
            : Math.Min(request.TamanoPagina, TamanoPaginaMaximo);

        var (pacientes, total) = await pacienteRepository.ObtenerPaginadoAsync(
            pagina, tamanoPagina, request.Busqueda, request.Estado, cancellationToken);

        var items = pacientes
            .Select(p => new PacienteDto(
                p.Id, p.Nombre, p.Apellido, p.Documento.Valor, p.Telefono, p.FotoCedulaPath is not null,
                p.Edad, p.Condicion, p.Estado, p.UltimaVisita))
            .ToList();

        return new PacientesPaginadosDto(items, total, pagina, tamanoPagina);
    }
}
