using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sistema;

namespace FUNBIDE.Application.UseCases.Sistema;

public interface IVerificarEspacioDiscoUseCase : IUseCase<EstadoDiscoDto>
{
}

/// <summary>Botón "Verificar espacio en disco" (LEMY, Mi Perfil > Avanzado). Ver <see cref="IEspacioDiscoService"/>.</summary>
public sealed class VerificarEspacioDiscoUseCase(IEspacioDiscoService espacioDisco) : IVerificarEspacioDiscoUseCase
{
    public Task<EstadoDiscoDto> EjecutarAsync(CancellationToken cancellationToken) =>
        espacioDisco.ObtenerEstadoAsync(cancellationToken);
}
