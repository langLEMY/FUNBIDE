using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Application.UseCases.Pacientes;

public interface IImportarPacientesUseCase : IUseCase<Stream, ImportarPacientesResultDto>
{
}

/// <summary>
/// Importa pacientes desde un Excel de un sistema anterior. A diferencia de
/// <see cref="CrearPacienteUseCase"/>, no rechaza identificaciones cortas ni
/// duplicadas — las ajusta automáticamente (ver <see cref="NormalizarIdentificacion"/>)
/// para poder migrar datos históricos imperfectos sin perder pacientes, en vez de
/// exigir que el archivo esté impecable antes de poder importarlo.
/// </summary>
public sealed class ImportarPacientesUseCase(
    IExcelLectorService excelLector,
    IPacienteRepository pacienteRepository) : IImportarPacientesUseCase
{
    public async Task<ImportarPacientesResultDto> EjecutarAsync(Stream request, CancellationToken cancellationToken)
    {
        var filas = excelLector.LeerFilas(request);

        var existentes = await pacienteRepository.ObtenerTodosAsync(cancellationToken);
        var identificacionesUsadas = existentes
            .Select(p => p.Documento.Valor)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var omisiones = new List<string>();
        var creados = 0;
        var ajustadas = 0;

        for (var i = 0; i < filas.Count; i++)
        {
            var fila = filas[i];
            var numeroFila = i + 2; // +1 por índice base 1, +1 por la fila de encabezado

            var nombreCompleto = fila.Buscar("Nombre", "Nombre completo");
            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                omisiones.Add($"Fila {numeroFila}: sin nombre, omitida.");
                continue;
            }

            var identificacionOriginal =
                fila.Buscar("Identificación", "Identificacion", "Cédula", "Cedula", "Documento")
                ?? $"SIN-ID-{numeroFila}";

            var identificacionFinal = NormalizarIdentificacion(identificacionOriginal, identificacionesUsadas);
            if (!string.Equals(identificacionFinal, identificacionOriginal.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ajustadas++;
            }

            var partesNombre = nombreCompleto.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var nombre = partesNombre[0];
            var apellido = partesNombre.Length > 1 ? partesNombre[1] : "-";

            var telefono = fila.Buscar("Teléfono", "Telefono", "Tel");
            var condicion = fila.Buscar("Condición", "Condicion");

            int? edad = null;
            var edadTexto = fila.Buscar("Edad");
            if (edadTexto is not null && double.TryParse(edadTexto, out var edadNumero) && edadNumero >= 0)
            {
                edad = (int)edadNumero;
            }

            var documento = DocumentoIdentidad.Crear(identificacionFinal);
            var paciente = new Paciente(nombre, apellido, documento, telefono, edad, condicion);

            await pacienteRepository.AgregarAsync(paciente, cancellationToken);
            creados++;
        }

        await pacienteRepository.GuardarCambiosAsync(cancellationToken);

        return new ImportarPacientesResultDto(filas.Count, creados, omisiones.Count, ajustadas, omisiones.Take(50).ToList());
    }

    /// <summary>
    /// Antepone "LEG-" a identificaciones demasiado cortas para pasar
    /// <see cref="DocumentoIdentidad.Crear"/> (mínimo 5 caracteres), y agrega un
    /// sufijo "-2", "-3"... a las que ya están en uso, sin superar los 20 caracteres.
    /// </summary>
    private static string NormalizarIdentificacion(string valorOriginal, HashSet<string> usadas)
    {
        var valor = valorOriginal.Trim();
        if (valor.Length < 5)
        {
            valor = $"LEG-{valor}";
        }

        if (valor.Length > 20)
        {
            valor = valor[..20];
        }

        if (usadas.Add(valor))
        {
            return valor;
        }

        var baseValor = valor.Length > 17 ? valor[..17] : valor;
        var contador = 2;
        string candidato;
        do
        {
            candidato = $"{baseValor}-{contador}";
            contador++;
        } while (!usadas.Add(candidato));

        return candidato;
    }
}
