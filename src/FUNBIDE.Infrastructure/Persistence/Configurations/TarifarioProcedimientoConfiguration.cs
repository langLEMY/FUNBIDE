using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FUNBIDE.Infrastructure.Persistence.Configurations;

public sealed class TarifarioProcedimientoConfiguration : IEntityTypeConfiguration<TarifarioProcedimiento>
{
    public void Configure(EntityTypeBuilder<TarifarioProcedimiento> builder)
    {
        builder.ToTable("tarifario_procedimientos");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.SeguroMedicoId).IsRequired();
        builder.Property(t => t.Plan).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Procedimiento).HasMaxLength(300).IsRequired();
        builder.Property(t => t.MontoSeguro).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(t => t.MontoPaciente).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(t => t.MontoTotal).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(t => t.MontoFondo).HasColumnType("decimal(10,2)");
        builder.Property(t => t.Especialidad).HasConversion<string>().HasMaxLength(40);
        builder.Property(t => t.Activo).IsRequired();

        // Reconciliación del import (ver ImportarTarifarioUseCase): si ya existe una fila
        // con el mismo seguro+plan+procedimiento, se actualizan los montos en vez de
        // duplicarla — necesario para poder reimportar una lista de precios actualizada.
        builder.HasIndex(t => new { t.SeguroMedicoId, t.Plan, t.Procedimiento }).IsUnique();

        // Tarifario de Aps (40 consultas, transcritas de "Tarifario de aps.docx"): el
        // paciente paga fijo RD$100 (RD$250 en Psicología Clínica) y el seguro cubre el
        // resto hasta el precio oficial de cada especialidad — de ahí que MontoSeguro
        // varíe por fila. Especialidad solo se completa cuando el nombre coincide sin
        // ambigüedad con una de las especialidades propias de FUNBIDE (EspecialidadMedica);
        // el resto (Dermatología, Nefrología, Otorrino, etc.) no tiene equivalente ahí y
        // queda null a propósito.
        //
        // Tarifario de Renacer: solo se confirmó UNA fila por chat (consulta general,
        // RD$750 pagados por Renacer, de los cuales RD$500 se reconocen como cobertura del
        // precio de referencia de RD$600 y los RD$250 restantes van al fondo interno de la
        // fundación — ver Cobro.MontoFondo). No se inventó el resto de su tarifario: falta
        // pedirle a Renacer el resto y cargarlo con el mismo importador (ya con columna
        // Fondo) desde Aseguradoras.
        builder.HasData(
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000001"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA MEDICINA GENERAL", MontoSeguro = 500.00m, MontoPaciente = 100.00m, MontoTotal = 600.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.MedicinaGeneralYFamiliar, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000002"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA MEDICINA ESPECIALIZADA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000003"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA PSIQUIATRICA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000004"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA PSICOLOGA CLINICA", MontoSeguro = 600.00m, MontoPaciente = 250.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.Psicologia, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000005"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA MEDICA QUIRURGICA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000006"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE CARDIOLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.Cardiologia, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000007"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DERMATOLOGICA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000008"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE ENDOCRINOLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000009"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE GASTROENTEROLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000010"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE GINECOLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.Ginecologia, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000011"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE HEMATOLOGIA", MontoSeguro = 1100.00m, MontoPaciente = 100.00m, MontoTotal = 1200.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000012"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE NEFROLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000013"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE NEUMOLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000014"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE NEUROCIRUGIA", MontoSeguro = 1100.00m, MontoPaciente = 100.00m, MontoTotal = 1200.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000015"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE NEUROLOGIA", MontoSeguro = 1100.00m, MontoPaciente = 100.00m, MontoTotal = 1200.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000016"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE OFTALMOLOGIA", MontoSeguro = 650.00m, MontoPaciente = 100.00m, MontoTotal = 750.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.Oftalmologia, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000017"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE ORTOPEDIA", MontoSeguro = 1100.00m, MontoPaciente = 100.00m, MontoTotal = 1200.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.Ortopedia, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000018"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE OTORRINOLARINGOLOGIA", MontoSeguro = 1100.00m, MontoPaciente = 100.00m, MontoTotal = 1200.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000019"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE PEDIATRIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.Pediatria, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000020"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE UROLOGIA", MontoSeguro = 860.00m, MontoPaciente = 100.00m, MontoTotal = 960.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000021"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE ONCOLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000022"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE GERIATRIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000023"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE REUMATOLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000024"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE DIABETOLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.Diabetologia, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000025"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE NUTRICION", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.Nutricion, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000026"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE INFECTOLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000027"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA MEDICINA INTERNA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.MedicinaInterna, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000028"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA CARDIO-VASCULAR", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000029"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE CIRUGIA PEDIATRICA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000030"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE ENDOCRINOLOGIA PEDIATRICA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000031"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE NEUMOLOGIA PEDIATRICA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000032"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE CIRUGIA GENERAL", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000033"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA PRE-ANESTESIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000034"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE MEDICINA FAMILIAR", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = EspecialidadMedica.MedicinaGeneralYFamiliar, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000035"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE PROCTOLOGIA", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000036"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE MAXILOFACIAL", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000037"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA ALERGISTA", MontoSeguro = 860.00m, MontoPaciente = 100.00m, MontoTotal = 960.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000038"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA DE NEONATOLOGIA", MontoSeguro = 860.00m, MontoPaciente = 100.00m, MontoTotal = 960.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000039"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA CLINICA DEL DOLOR", MontoSeguro = 750.00m, MontoPaciente = 100.00m, MontoTotal = 850.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000040"), SeguroMedicoId = SeguroMedicoIds.Aps, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA MEDICINA PIE DIABETICO", MontoSeguro = 740.00m, MontoPaciente = 100.00m, MontoTotal = 840.00m, MontoFondo = (decimal?)null, Especialidad = (EspecialidadMedica?)null, Activo = true },
            new { Id = Guid.Parse("8f3c7d20-0004-4a3e-8a9a-000000000041"), SeguroMedicoId = SeguroMedicoIds.Renacer, Plan = PlanAseguradora.Estandar, Procedimiento = "CONSULTA MEDICINA GENERAL", MontoSeguro = 500.00m, MontoPaciente = 100.00m, MontoTotal = 600.00m, MontoFondo = (decimal?)250.00m, Especialidad = EspecialidadMedica.MedicinaGeneralYFamiliar, Activo = true });
    }
}
