using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class EmpleadoRepository(FunbideDbContext dbContext) : IEmpleadoRepository
{
    public async Task<IReadOnlyList<Empleado>> ObtenerTodosAsync(CancellationToken cancellationToken) =>
        await dbContext.Empleados.AsNoTracking().OrderBy(e => e.NombreCompleto).ToListAsync(cancellationToken);

    public Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Empleados.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AgregarAsync(Empleado empleado, CancellationToken cancellationToken) =>
        await dbContext.Empleados.AddAsync(empleado, cancellationToken);

    public void Eliminar(Empleado empleado) => dbContext.Empleados.Remove(empleado);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
