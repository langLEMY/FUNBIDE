using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Dashboard;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Dashboard;

public interface IObtenerAlertasAdminUseCase : IUseCase<AlertasAdminDto>
{
}

/// <summary>
/// Alimenta la sección "Alertas" del Dashboard de Admin: items de inventario por debajo
/// de su stock mínimo, y pacientes con saldo pendiente de cobro. Ninguno de los dos se
/// calculaba antes en el backend (stock bajo se calculaba solo en el frontend de
/// Inventario; deuda solo se contaba, nunca se listaba con nombre y monto).
/// </summary>
public sealed class ObtenerAlertasAdminUseCase(
    IInventarioRepository inventarioRepository,
    ICobroRepository cobroRepository,
    IPacienteRepository pacienteRepository) : IObtenerAlertasAdminUseCase
{
    public async Task<AlertasAdminDto> EjecutarAsync(CancellationToken cancellationToken)
    {
        var items = await inventarioRepository.ObtenerTodosAsync(cancellationToken);
        var stockBajo = items
            .Where(i => i.StockActual < i.StockMinimo)
            .Select(i => new ItemStockBajoDto(i.Id, i.Codigo, i.Nombre, i.StockActual, i.StockMinimo))
            .OrderBy(i => i.StockActual - i.StockMinimo)
            .ToList();

        var deudaPorPaciente = await cobroRepository.ObtenerTodosConDeudaAsync(cancellationToken);
        var nombresPacientes = await pacienteRepository.ObtenerNombresPorIdsAsync(deudaPorPaciente.Keys.ToList(), cancellationToken);
        var pacientesConDeuda = deudaPorPaciente
            .Select(par => new PacienteConDeudaDto(
                par.Key, nombresPacientes.GetValueOrDefault(par.Key, "Paciente desconocido"), par.Value))
            .OrderByDescending(p => p.MontoAdeudado)
            .ToList();

        return new AlertasAdminDto(stockBajo, pacientesConDeuda);
    }
}
