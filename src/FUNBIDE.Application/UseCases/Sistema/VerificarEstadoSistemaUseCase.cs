using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sistema;

namespace FUNBIDE.Application.UseCases.Sistema;

public interface IVerificarEstadoSistemaUseCase : IUseCase<EstadoSistemaDto>
{
}

/// <summary>
/// Alimenta el panel "Estado del sistema" exclusivo de LEMY. Por ahora solo reporta
/// conectividad a la base de datos — es el único indicador para el que existe una
/// fuente de verdad real hoy; no se fabrican datos para respaldos o sincronización
/// sin un mecanismo real detrás.
/// </summary>
public sealed class VerificarEstadoSistemaUseCase(
    IEstadoBaseDeDatosService estadoBaseDeDatos) : IVerificarEstadoSistemaUseCase
{
    public async Task<EstadoSistemaDto> EjecutarAsync(CancellationToken cancellationToken)
    {
        var operativa = await estadoBaseDeDatos.VerificarConexionAsync(cancellationToken);
        return new EstadoSistemaDto(operativa);
    }
}
