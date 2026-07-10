using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Interfaces;

public interface IPacienteRepository
{
    Task<(IReadOnlyList<Paciente> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, string? busqueda, EstadoPaciente? estado, CancellationToken cancellationToken);

    /// <summary>
    /// Carga completa y con seguimiento de cambios (sin AsNoTracking), solo para que
    /// <c>ImportarPacientesUseCase</c> pueda reconciliar el archivo contra los pacientes
    /// existentes (por nombre y apellido) y actualizarlos en la misma unidad de trabajo.
    /// No usar en rutas de solo lectura: para eso está <see cref="ObtenerPaginadoAsync"/>.
    /// </summary>
    Task<IReadOnlyList<Paciente>> ObtenerTodosParaImportarAsync(CancellationToken cancellationToken);

    Task<Paciente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Paciente?> ObtenerPorDocumentoAsync(string documento, CancellationToken cancellationToken);

    Task AgregarAsync(Paciente paciente, CancellationToken cancellationToken);

    void Eliminar(Paciente paciente);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
