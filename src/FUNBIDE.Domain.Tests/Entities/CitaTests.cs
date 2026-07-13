using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Domain.Tests.Entities;

public class CitaTests
{
    private static Cita CrearCita() => new(Guid.NewGuid(), Guid.NewGuid(), "Consulta general");

    private static IntervaloCita CrearIntervaloFuturo() =>
        IntervaloCita.Crear(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(1));

    [Fact]
    public void Constructor_MotivoVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Cita(Guid.NewGuid(), Guid.NewGuid(), "  "));
    }

    [Fact]
    public void Constructor_CitaNueva_QuedaEnEstadoPendiente()
    {
        var cita = CrearCita();

        Assert.Equal(EstadoCita.Pendiente, cita.Estado);
    }

    [Fact]
    public void Programar_DesdeCitaPendiente_CambiaEstadoAProgramada()
    {
        var cita = CrearCita();

        cita.Programar(CrearIntervaloFuturo());

        Assert.Equal(EstadoCita.Programada, cita.Estado);
    }

    [Fact]
    public void Programar_DesdeCitaYaProgramada_LanzaInvalidOperationException()
    {
        var cita = CrearCita();
        cita.Programar(CrearIntervaloFuturo());

        Assert.Throws<InvalidOperationException>(() => cita.Programar(CrearIntervaloFuturo()));
    }

    [Fact]
    public void Completar_DesdeCitaProgramada_CambiaEstadoACompletadaYGuardaNotas()
    {
        var cita = CrearCita();
        cita.Programar(CrearIntervaloFuturo());

        cita.Completar("Paciente estable, se receta reposo.");

        Assert.Equal(EstadoCita.Completada, cita.Estado);
        Assert.Equal("Paciente estable, se receta reposo.", cita.NotasCierre);
    }

    [Fact]
    public void Completar_DesdeCitaPendiente_LanzaInvalidOperationException()
    {
        var cita = CrearCita();

        Assert.Throws<InvalidOperationException>(() => cita.Completar("Notas"));
    }

    [Fact]
    public void RegistrarLlegada_DesdeCitaProgramada_CambiaEstadoAEnEspera()
    {
        var cita = CrearCita();
        cita.Programar(CrearIntervaloFuturo());

        cita.RegistrarLlegada();

        Assert.Equal(EstadoCita.EnEspera, cita.Estado);
    }

    [Fact]
    public void RegistrarLlegada_DesdeCitaPendiente_CambiaEstadoAEnEspera()
    {
        var cita = CrearCita();

        cita.RegistrarLlegada();

        Assert.Equal(EstadoCita.EnEspera, cita.Estado);
    }

    [Fact]
    public void RegistrarLlegada_DesdeCitaCompletada_LanzaInvalidOperationException()
    {
        var cita = CrearCita();
        cita.Programar(CrearIntervaloFuturo());
        cita.Completar("Notas de cierre");

        Assert.Throws<InvalidOperationException>(() => cita.RegistrarLlegada());
    }

    [Fact]
    public void Completar_DesdeCitaEnEspera_CambiaEstadoACompletada()
    {
        var cita = CrearCita();
        cita.Programar(CrearIntervaloFuturo());
        cita.RegistrarLlegada();

        cita.Completar("Paciente estable, se receta reposo.");

        Assert.Equal(EstadoCita.Completada, cita.Estado);
    }

    [Fact]
    public void Cancelar_DesdeCitaCompletada_LanzaInvalidOperationException()
    {
        var cita = CrearCita();
        cita.Programar(CrearIntervaloFuturo());
        cita.Completar("Notas de cierre");

        Assert.Throws<InvalidOperationException>(() => cita.Cancelar());
    }

    [Fact]
    public void Cancelar_DesdeCitaPendiente_CambiaEstadoACancelada()
    {
        var cita = CrearCita();

        cita.Cancelar();

        Assert.Equal(EstadoCita.Cancelada, cita.Estado);
    }
}
