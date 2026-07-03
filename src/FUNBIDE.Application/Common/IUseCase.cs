namespace FUNBIDE.Application.Common;

/// <summary>
/// Contrato uniforme para un caso de uso con entrada y salida. Cada caso de uso
/// tiene una única responsabilidad (SRP) y se resuelve por DI en los controladores,
/// sin necesidad de un mediador genérico.
/// </summary>
public interface IUseCase<in TRequest, TResponse>
{
    Task<TResponse> EjecutarAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>Caso de uso sin parámetros de entrada.</summary>
public interface IUseCase<TResponse>
{
    Task<TResponse> EjecutarAsync(CancellationToken cancellationToken);
}
