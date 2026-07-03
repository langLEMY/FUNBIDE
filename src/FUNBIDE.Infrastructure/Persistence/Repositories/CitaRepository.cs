using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class CitaRepository(FunbideDbContext dbContext) : ICitaRepository
{
    public Task<Cita?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Citas.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Cita>> ObtenerPorDoctorYEstadoAsync(
        Guid doctorId, EstadoCita estado, CancellationToken cancellationToken) =>
        await dbContext.Citas
            .AsNoTracking()
            .Where(c => c.DoctorId == doctorId && c.Estado == estado)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteAlgunaParaPacienteAsync(Guid pacienteId, CancellationToken cancellationToken) =>
        dbContext.Citas.AnyAsync(c => c.PacienteId == pacienteId, cancellationToken);

    public async Task AgregarAsync(Cita cita, CancellationToken cancellationToken) =>
        await dbContext.Citas.AddAsync(cita, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
