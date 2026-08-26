using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class CitaRepository(FunbideDbContext dbContext, IDateTimeProvider dateTimeProvider) : ICitaRepository
{
    // El corte de "día" se calcula con el offset de la clínica, no en UTC (offset cero):
    // de lo contrario una cita de la noche local cae en el día UTC siguiente y desaparece
    // de Agenda/Sala de Espera de "hoy". America/Santo_Domingo no tiene horario de
    // verano, así que el offset es siempre el mismo, pero se pide igual vía TimeZoneInfo
    // en vez de hardcodear "-4" para que quede explícito de dónde sale. Se normaliza a
    // UTC (.ToUniversalTime()) antes de usarlo como parámetro de la consulta — mismo
    // instante, pero Npgsql rechaza un DateTimeOffset con offset distinto de cero al
    // compararlo contra una columna timestamptz.
    private DateTimeOffset InicioDeDiaLocal(DateOnly dia)
    {
        var mediaNocheIngenua = dia.ToDateTime(TimeOnly.MinValue);
        var offset = dateTimeProvider.ZonaHorariaClinica.GetUtcOffset(mediaNocheIngenua);
        return new DateTimeOffset(mediaNocheIngenua, offset).ToUniversalTime();
    }

    public Task<Cita?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Citas.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Cita>> ObtenerPorDoctorYEstadoAsync(
        Guid doctorId, EstadoCita estado, CancellationToken cancellationToken) =>
        await dbContext.Citas
            .AsNoTracking()
            .Where(c => c.DoctorId == doctorId && c.Estado == estado)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> ObtenerPacienteIdsDistintosPorDoctorAsync(Guid doctorId, CancellationToken cancellationToken) =>
        await dbContext.Citas
            .AsNoTracking()
            .Where(c => c.DoctorId == doctorId)
            .Select(c => c.PacienteId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<(Guid DoctorId, Guid PacienteId)>> ObtenerDoctorYPacientePorCompletadasAsync(CancellationToken cancellationToken)
    {
        var filas = await dbContext.Citas
            .AsNoTracking()
            .Where(c => c.Estado == EstadoCita.Completada)
            .Select(c => new { c.DoctorId, c.PacienteId })
            .ToListAsync(cancellationToken);

        return filas.Select(f => (f.DoctorId, f.PacienteId)).ToList();
    }

    public async Task<IReadOnlyDictionary<Guid, Guid>> ObtenerDoctorIdsPorCitaIdsAsync(
        IReadOnlyCollection<Guid> citaIds, CancellationToken cancellationToken) =>
        (await dbContext.Citas
            .AsNoTracking()
            .Where(c => citaIds.Contains(c.Id))
            .Select(c => new { c.Id, c.DoctorId })
            .ToListAsync(cancellationToken))
            .ToDictionary(c => c.Id, c => c.DoctorId);

    public Task<bool> ExisteAlgunaParaPacienteAsync(Guid pacienteId, CancellationToken cancellationToken) =>
        dbContext.Citas.AnyAsync(c => c.PacienteId == pacienteId, cancellationToken);

    public Task<bool> TieneCitaActivaAsync(Guid pacienteId, Guid doctorId, CancellationToken cancellationToken) =>
        dbContext.Citas.AnyAsync(c =>
            c.PacienteId == pacienteId && c.DoctorId == doctorId &&
            (c.Estado == EstadoCita.Programada || c.Estado == EstadoCita.EnEspera),
            cancellationToken);

    public async Task<IReadOnlyList<Cita>> ObtenerPorFiltroAsync(
        DateOnly? fecha, Guid? doctorId, CancellationToken cancellationToken)
    {
        var query = dbContext.Citas.AsNoTracking().AsQueryable();

        if (doctorId is not null)
        {
            query = query.Where(c => c.DoctorId == doctorId);
        }

        if (fecha is not null)
        {
            var inicioDia = InicioDeDiaLocal(fecha.Value);
            var finDia = inicioDia.AddDays(1);
            query = query.Where(c => c.Intervalo != null && c.Intervalo.Inicio >= inicioDia && c.Intervalo.Inicio < finDia);
        }

        return await query.OrderBy(c => c.Intervalo!.Inicio).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cita>> ObtenerSalaDeEsperaAsync(DateOnly hoy, CancellationToken cancellationToken)
    {
        var inicioDia = InicioDeDiaLocal(hoy);
        var finDia = inicioDia.AddDays(1);

        return await dbContext.Citas
            .AsNoTracking()
            .Where(c => c.Estado == EstadoCita.EnEspera ||
                (c.Estado == EstadoCita.Programada && c.Intervalo != null &&
                 c.Intervalo.Inicio >= inicioDia && c.Intervalo.Inicio < finDia))
            .OrderBy(c => c.Intervalo!.Inicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cita>> ObtenerPendientesDeCobroAsync(CancellationToken cancellationToken) =>
        await dbContext.Citas
            .AsNoTracking()
            .Where(c => c.Estado == EstadoCita.Completada && !dbContext.Cobros.Any(cob => cob.CitaId == c.Id))
            .ToListAsync(cancellationToken);

    public Task<bool> TieneChoqueDeHorarioAsync(
        Guid doctorId, DateTimeOffset inicio, DateTimeOffset fin, Guid? excluirCitaId, CancellationToken cancellationToken) =>
        dbContext.Citas.AnyAsync(c =>
            c.DoctorId == doctorId &&
            c.Id != excluirCitaId &&
            (c.Estado == EstadoCita.Programada || c.Estado == EstadoCita.EnEspera) &&
            c.Intervalo != null && c.Intervalo.Inicio < fin && c.Intervalo.Fin > inicio,
            cancellationToken);

    public async Task AgregarAsync(Cita cita, CancellationToken cancellationToken) =>
        await dbContext.Citas.AddAsync(cita, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
