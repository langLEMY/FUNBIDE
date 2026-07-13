using FluentValidation;
using FUNBIDE.Application.UseCases.Auditoria;
using FUNBIDE.Application.UseCases.Auth;
using FUNBIDE.Application.UseCases.Caja;
using FUNBIDE.Application.UseCases.Citas;
using FUNBIDE.Application.UseCases.Cobros;
using FUNBIDE.Application.UseCases.Dashboard;
using FUNBIDE.Application.UseCases.Empleados;
using FUNBIDE.Application.UseCases.Finanzas;
using FUNBIDE.Application.UseCases.FinanzasAdmin;
using FUNBIDE.Application.UseCases.HistorialClinico;
using FUNBIDE.Application.UseCases.Inventario;
using FUNBIDE.Application.UseCases.Pacientes;
using FUNBIDE.Application.UseCases.Personal;
using FUNBIDE.Application.UseCases.SegurosMedicos;
using FUNBIDE.Application.UseCases.Sistema;

namespace FUNBIDE.API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra los casos de uso de Application explícitamente: un caso de uso, una
    /// interfaz, una implementación. Nada de escaneo de ensamblados "mágico" que
    /// oculte qué se está resolviendo.
    /// </summary>
    public static IServiceCollection AddFunbideUseCases(this IServiceCollection services)
    {
        services.AddScoped<IObtenerCitasPorEstadoUseCase, ObtenerCitasPorEstadoUseCase>();
        services.AddScoped<ICrearCitaUseCase, CrearCitaUseCase>();
        services.AddScoped<IProgramarCitaUseCase, ProgramarCitaUseCase>();
        services.AddScoped<ICompletarCitaUseCase, CompletarCitaUseCase>();
        services.AddScoped<ICancelarCitaUseCase, CancelarCitaUseCase>();
        services.AddScoped<IAgendarCitaUseCase, AgendarCitaUseCase>();
        services.AddScoped<IRegistrarLlegadaUseCase, RegistrarLlegadaUseCase>();
        services.AddScoped<IRegistrarLlegadaSinCitaUseCase, RegistrarLlegadaSinCitaUseCase>();
        services.AddScoped<IListarAgendaUseCase, ListarAgendaUseCase>();
        services.AddScoped<IListarSalaDeEsperaUseCase, ListarSalaDeEsperaUseCase>();
        services.AddScoped<IListarPendientesDeCobroUseCase, ListarPendientesDeCobroUseCase>();

        services.AddScoped<IAbrirTurnoCajaUseCase, AbrirTurnoCajaUseCase>();
        services.AddScoped<ICerrarTurnoCajaUseCase, CerrarTurnoCajaUseCase>();
        services.AddScoped<IObtenerTurnoCajaActualUseCase, ObtenerTurnoCajaActualUseCase>();
        services.AddScoped<IObtenerResumenCajaUseCase, ObtenerResumenCajaUseCase>();

        services.AddScoped<IRegistrarCobroUseCase, RegistrarCobroUseCase>();
        services.AddScoped<IListarCobrosDelTurnoUseCase, ListarCobrosDelTurnoUseCase>();
        services.AddScoped<IObtenerDeudaPacienteUseCase, ObtenerDeudaPacienteUseCase>();

        services.AddScoped<IListarSegurosMedicosUseCase, ListarSegurosMedicosUseCase>();
        services.AddScoped<ICrearSeguroMedicoUseCase, CrearSeguroMedicoUseCase>();
        services.AddScoped<IEditarSeguroMedicoUseCase, EditarSeguroMedicoUseCase>();
        services.AddScoped<IDesactivarSeguroMedicoUseCase, DesactivarSeguroMedicoUseCase>();
        services.AddScoped<IReactivarSeguroMedicoUseCase, ReactivarSeguroMedicoUseCase>();

        services.AddScoped<IRegistrarEntradaHistorialUseCase, RegistrarEntradaHistorialUseCase>();
        services.AddScoped<IObtenerHistorialPorPacienteUseCase, ObtenerHistorialPorPacienteUseCase>();

        services.AddScoped<IDescargarInventarioUseCase, DescargarInventarioUseCase>();
        services.AddScoped<IListarInventarioUseCase, ListarInventarioUseCase>();
        services.AddScoped<ICrearInventarioItemUseCase, CrearInventarioItemUseCase>();
        services.AddScoped<IEditarInventarioItemUseCase, EditarInventarioItemUseCase>();
        services.AddScoped<IEliminarInventarioItemUseCase, EliminarInventarioItemUseCase>();

        services.AddScoped<IObtenerLogsAuditoriaUseCase, ObtenerLogsAuditoriaUseCase>();
        services.AddScoped<IVerificarEstadoSistemaUseCase, VerificarEstadoSistemaUseCase>();

        services.AddScoped<IRegistrarEventoLoginUseCase, RegistrarEventoLoginUseCase>();
        services.AddScoped<IIniciarSesionLocalUseCase, IniciarSesionLocalUseCase>();

        services.AddScoped<IListarPersonalUseCase, ListarPersonalUseCase>();
        services.AddScoped<IListarDoctoresUseCase, ListarDoctoresUseCase>();
        services.AddScoped<ICrearUsuarioUseCase, CrearUsuarioUseCase>();
        services.AddScoped<IEditarUsuarioUseCase, EditarUsuarioUseCase>();
        services.AddScoped<ICambiarRolUsuarioUseCase, CambiarRolUsuarioUseCase>();
        services.AddScoped<ICambiarContrasenaUsuarioUseCase, CambiarContrasenaUsuarioUseCase>();
        services.AddScoped<IActualizarFotoPerfilUseCase, ActualizarFotoPerfilUseCase>();
        services.AddScoped<IEliminarUsuarioUseCase, EliminarUsuarioUseCase>();
        services.AddScoped<IEliminarUsuarioPermanentementeUseCase, EliminarUsuarioPermanentementeUseCase>();
        services.AddScoped<IReactivarUsuarioUseCase, ReactivarUsuarioUseCase>();
        services.AddScoped<IVerPerfilPropioUseCase, VerPerfilPropioUseCase>();
        services.AddScoped<IActualizarFotoPerfilPropiaUseCase, ActualizarFotoPerfilPropiaUseCase>();
        services.AddScoped<ICambiarMiContrasenaUseCase, CambiarMiContrasenaUseCase>();
        services.AddScoped<IActualizarMiPerfilUseCase, ActualizarMiPerfilUseCase>();

        services.AddScoped<IRegistrarMovimientoFinancieroUseCase, RegistrarMovimientoFinancieroUseCase>();
        services.AddScoped<IListarMovimientosFinancierosUseCase, ListarMovimientosFinancierosUseCase>();

        services.AddScoped<IObtenerResumenHoyUseCase, ObtenerResumenHoyUseCase>();
        services.AddScoped<IObtenerResumenMesUseCase, ObtenerResumenMesUseCase>();

        services.AddScoped<IListarMovimientosImportantesUseCase, ListarMovimientosImportantesUseCase>();
        services.AddScoped<IObtenerResumenAnualUseCase, ObtenerResumenAnualUseCase>();
        services.AddScoped<IRegistrarGastoAdminUseCase, RegistrarGastoAdminUseCase>();

        services.AddScoped<IListarEmpleadosUseCase, ListarEmpleadosUseCase>();
        services.AddScoped<ICrearEmpleadoUseCase, CrearEmpleadoUseCase>();
        services.AddScoped<IEditarEmpleadoUseCase, EditarEmpleadoUseCase>();
        services.AddScoped<IEliminarEmpleadoUseCase, EliminarEmpleadoUseCase>();
        services.AddScoped<IImportarEmpleadosUseCase, ImportarEmpleadosUseCase>();

        services.AddScoped<IListarPacientesUseCase, ListarPacientesUseCase>();
        services.AddScoped<IObtenerPacientePorIdUseCase, ObtenerPacientePorIdUseCase>();
        services.AddScoped<ICrearPacienteUseCase, CrearPacienteUseCase>();
        services.AddScoped<IEditarPacienteUseCase, EditarPacienteUseCase>();
        services.AddScoped<IEliminarPacienteUseCase, EliminarPacienteUseCase>();
        services.AddScoped<IActualizarFotoCedulaUseCase, ActualizarFotoCedulaUseCase>();
        services.AddScoped<IObtenerUrlFotoCedulaUseCase, ObtenerUrlFotoCedulaUseCase>();
        services.AddScoped<IImportarPacientesUseCase, ImportarPacientesUseCase>();

        return services;
    }

    public static IServiceCollection AddFunbideValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IObtenerCitasPorEstadoUseCase>();
        return services;
    }
}
