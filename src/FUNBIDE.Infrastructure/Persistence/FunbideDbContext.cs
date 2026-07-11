using FUNBIDE.Domain.Entities;
using FUNBIDE.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence;

public sealed class FunbideDbContext(DbContextOptions<FunbideDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<CredencialLocal> CredencialesLocales => Set<CredencialLocal>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<EntradaHistorialClinico> HistorialClinico => Set<EntradaHistorialClinico>();
    public DbSet<InventarioItem> InventarioItems => Set<InventarioItem>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();
    public DbSet<MovimientoFinanciero> MovimientosFinancieros => Set<MovimientoFinanciero>();
    public DbSet<ResumenDiario> ResumenesDiarios => Set<ResumenDiario>();
    public DbSet<Empleado> Empleados => Set<Empleado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FunbideDbContext).Assembly);
        modelBuilder.HasDefaultSchema("funbide");
    }
}
