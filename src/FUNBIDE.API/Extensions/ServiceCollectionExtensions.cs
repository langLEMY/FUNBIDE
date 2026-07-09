using FluentValidation;
using FUNBIDE.Application.UseCases.Auditoria;
using FUNBIDE.Application.UseCases.Auth;
using FUNBIDE.Application.UseCases.Citas;
using FUNBIDE.Application.UseCases.Dashboard;
using FUNBIDE.Application.UseCases.Empleados;
using FUNBIDE.Application.UseCases.Finanzas;
using FUNBIDE.Application.UseCases.HistorialClinico;
using FUNBIDE.Application.UseCases.Inventario;
using FUNBIDE.Application.UseCases.Pacientes;
using FUNBIDE.Application.UseCases.Personal;
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

        services.AddScoped<IRegistrarEntradaHistorialUseCase, RegistrarEntradaHistorialUseCase>();
        services.AddScoped<IObtenerHistorialPorPacienteUseCase, ObtenerHistorialPorPacienteUseCase>();

        services.AddScoped<IDescargarInventarioUseCase, DescargarInventarioUseCase>();
        services.AddScoped<IListarInventarioUseCase, ListarInventarioUseCase>();
        services.AddScoped<ICrearInventarioItemUseCase, CrearInventarioItemUseCase>();
        services.AddScoped<IEditarInventarioItemUseCase, EditarInventarioItemUseCase>();

        services.AddScoped<IObtenerLogsAuditoriaUseCase, ObtenerLogsAuditoriaUseCase>();
        services.AddScoped<IVerificarEstadoSistemaUseCase, VerificarEstadoSistemaUseCase>();

        services.AddScoped<IRegistrarEventoLoginUseCase, RegistrarEventoLoginUseCase>();

        services.AddScoped<IListarPersonalUseCase, ListarPersonalUseCase>();
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

        services.AddScoped<IRegistrarMovimientoFinancieroUseCase, RegistrarMovimientoFinancieroUseCase>();
        services.AddScoped<IListarMovimientosFinancierosUseCase, ListarMovimientosFinancierosUseCase>();

        services.AddScoped<IObtenerResumenHoyUseCase, ObtenerResumenHoyUseCase>();
        services.AddScoped<IObtenerResumenMesUseCase, ObtenerResumenMesUseCase>();

        services.AddScoped<IListarEmpleadosUseCase, ListarEmpleadosUseCase>();
        services.AddScoped<ICrearEmpleadoUseCase, CrearEmpleadoUseCase>();
        services.AddScoped<IEditarEmpleadoUseCase, EditarEmpleadoUseCase>();
        services.AddScoped<IEliminarEmpleadoUseCase, EliminarEmpleadoUseCase>();
        services.AddScoped<IImportarEmpleadosUseCase, ImportarEmpleadosUseCase>();

        services.AddScoped<IListarPacientesUseCase, ListarPacientesUseCase>();
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
