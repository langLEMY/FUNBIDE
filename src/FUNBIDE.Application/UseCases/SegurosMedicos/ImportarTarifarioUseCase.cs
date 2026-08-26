using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.SegurosMedicos;

public interface IImportarTarifarioUseCase : IUseCase<ImportarTarifarioRequest, ImportarTarifarioResultDto>
{
}

/// <summary>
/// Importa el tarifario de un plan puntual (ver <see cref="ImportarTarifarioRequest"/>: el
/// Excel de origen trae una hoja por plan, no una columna de plan). Espera una hoja plana
/// con columnas Procedimiento/Seguro/Paciente/Total — no reconstruye layouts "para
/// imprimir" con secciones y encabezados repetidos a mitad de hoja; para eso hay que
/// aplanar el archivo primero. Reconcilia por nombre de procedimiento (normalizado) contra
/// lo ya cargado para ese seguro+plan: si existe, actualiza montos; si no, crea.
/// </summary>
public sealed class ImportarTarifarioUseCase(
    IExcelLectorService excelLector,
    ITarifarioProcedimientoRepository tarifarioRepository) : IImportarTarifarioUseCase
{
    public async Task<ImportarTarifarioResultDto> EjecutarAsync(
        ImportarTarifarioRequest request, CancellationToken cancellationToken)
    {
        var filas = excelLector.LeerFilas(request.Contenido);

        var existentes = await tarifarioRepository.ObtenerParaImportarAsync(
            request.SeguroMedicoId, request.Plan, cancellationToken);
        var porProcedimiento = new Dictionary<string, TarifarioProcedimiento>(StringComparer.OrdinalIgnoreCase);
        foreach (var fila in existentes)
        {
            porProcedimiento[fila.Procedimiento] = fila;
        }

        var omisiones = new List<string>();
        var creados = 0;
        var actualizados = 0;

        for (var i = 0; i < filas.Count; i++)
        {
            var fila = filas[i];
            var numeroFila = i + 2; // +1 por índice base 1, +1 por la fila de encabezado

            var procedimiento = fila.Buscar("Procedimiento", "Consulta", "Nombre", "Descripción", "Descripcion")?.Trim();
            if (string.IsNullOrWhiteSpace(procedimiento))
            {
                omisiones.Add($"Fila {numeroFila}: sin nombre de procedimiento, omitida.");
                continue;
            }

            var montoSeguro = LeerMonto(fila.Buscar("Seguro", "Monto Seguro", "Cubre Seguro"));
            var montoPaciente = LeerMonto(fila.Buscar("Paciente", "Monto Paciente", "Copago"));
            var montoTotal = LeerMonto(fila.Buscar("Total", "Precio Total", "Monto Total"));
            // Opcional (hoy solo Renacer lo usa): excedente que la aseguradora paga por
            // encima de MontoSeguro y que queda como ingreso interno de la fundación en vez
            // de reconocérsele al paciente — ver TarifarioProcedimiento.MontoFondo.
            var montoFondo = LeerMonto(fila.Buscar("Fondo", "Monto Fondo", "Interno"));
            var especialidad = LeerEspecialidad(fila.Buscar("Especialidad"));

            if (montoSeguro is null || montoPaciente is null)
            {
                omisiones.Add($"Fila {numeroFila} ({procedimiento}): faltan los montos de seguro o paciente, omitida.");
                continue;
            }

            // El total viene explícito en la mayoría de las filas del tarifario real, pero
            // alguna fila del origen lo trae en blanco pese a tener seguro+paciente
            // completos: se deriva en vez de descartar la fila entera por eso.
            var totalFinal = montoTotal ?? montoSeguro.Value + montoPaciente.Value;

            if (porProcedimiento.TryGetValue(procedimiento, out var filaExistente))
            {
                filaExistente.ActualizarMontos(montoSeguro.Value, montoPaciente.Value, totalFinal, montoFondo);
                if (especialidad.HasValue)
                {
                    filaExistente.AsignarEspecialidad(especialidad);
                }
                actualizados++;
                continue;
            }

            var nuevaFila = new TarifarioProcedimiento(
                request.SeguroMedicoId, request.Plan, procedimiento, montoSeguro.Value, montoPaciente.Value, totalFinal,
                montoFondo, especialidad);
            await tarifarioRepository.AgregarAsync(nuevaFila, cancellationToken);
            porProcedimiento[procedimiento] = nuevaFila;
            creados++;
        }

        await tarifarioRepository.GuardarCambiosAsync(cancellationToken);

        return new ImportarTarifarioResultDto(filas.Count, creados, actualizados, omisiones.Count, omisiones.Take(50).ToList());
    }

    private static decimal? LeerMonto(string? texto) =>
        texto is not null && decimal.TryParse(texto, out var monto) && monto >= 0 ? monto : null;

    private static EspecialidadMedica? LeerEspecialidad(string? texto) =>
        texto is not null && Enum.TryParse<EspecialidadMedica>(texto.Trim(), ignoreCase: true, out var especialidad)
            ? especialidad
            : null;
}
