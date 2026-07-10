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
            var patron = $"%{busqueda.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Nombre, patron) ||
                EF.Functions.ILike(p.Apellido, patron) ||
                (p.Condicion != null && EF.Functions.ILike(p.Condicion, patron)));
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

    public async Task AgregarAsync(Paciente paciente, CancellationToken cancellationToken) =>
        await dbContext.Pacientes.AddAsync(paciente, cancellationToken);

    public void Eliminar(Paciente paciente) => dbContext.Pacientes.Remove(paciente);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
