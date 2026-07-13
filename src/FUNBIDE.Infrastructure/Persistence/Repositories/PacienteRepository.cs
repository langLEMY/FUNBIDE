using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class PacienteRepository(FunbideDbContext dbContext) : IPacienteRepository
{
    public async Task<(IReadOnlyList<Paciente> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, string? busqueda, EstadoPaciente? estado, CancellationToken cancellationToken)
    {
        var query = dbContext.Pacientes.AsNoTracking();

        if (estado is not null)
        {
            query = query.Where(p => p.Estado == estado);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            // Escapar los comodines de LIKE (%, _) y el propio carácter de escape antes de
            // envolver en %...%: sin esto, alguien que busca "50%" o "a_b" literal recibe
            // coincidencias más amplias de las esperadas (% y _ se interpretan como
            // comodines reales en vez de texto literal).
            var textoEscapado = busqueda.Trim()
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
            var patron = $"%{textoEscapado}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Nombre, patron, "\\") ||
                EF.Functions.ILike(p.Apellido, patron, "\\") ||
                (p.Condicion != null && EF.Functions.ILike(p.Condicion, patron, "\\")));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Nombre)
            .ThenBy(p => p.Apellido)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<Paciente>> ObtenerTodosParaImportarAsync(CancellationToken cancellationToken) =>
        await dbContext.Pacientes.ToListAsync(cancellationToken);

    public Task<Paciente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Pacientes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Paciente?> ObtenerPorDocumentoAsync(string documento, CancellationToken cancellationToken)
    {
        // No se puede comparar p.Documento.Valor directamente: EF Core no traduce el
        // acceso a un miembro de un Value Object convertido (HasConversion) dentro del
        // predicado. Comparando el Value Object completo sí se traduce, porque EF aplica
        // la misma conversión a ambos lados antes de generar el SQL.
        var documentoIdentidad = DocumentoIdentidad.Crear(documento);
        return dbContext.Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Documento == documentoIdentidad, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> ObtenerNombresPorIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        // Se proyectan Nombre/Apellido (columnas reales) y se concatenan en memoria: la
        // interpolación de Paciente.NombreCompleto es una propiedad calculada en C#, no
        // una columna, y EF Core no puede traducirla dentro de la consulta.
        var pacientes = await dbContext.Pacientes
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new { p.Id, p.Nombre, p.Apellido })
            .ToListAsync(cancellationToken);

        return pacientes.ToDictionary(p => p.Id, p => $"{p.Nombre} {p.Apellido}");
    }

    public async Task AgregarAsync(Paciente paciente, CancellationToken cancellationToken) =>
        await dbContext.Pacientes.AddAsync(paciente, cancellationToken);

    public void Eliminar(Paciente paciente) => dbContext.Pacientes.Remove(paciente);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
