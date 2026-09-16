using Npgsql;
using Polly;
using Polly.Retry;

namespace FUNBIDE.Infrastructure.Resiliencia;

/// <summary>
/// Reintento corto con backoff, SOLO para operaciones de lectura contra Postgres/Supabase
/// ante fallos de red transitorios (el tipo de corte breve que ya se resuelve solo medio
/// segundo después). Nunca se usa para escrituras a propósito: reintentar automáticamente
/// una escritura podría reintroducir el problema de duplicados que la idempotencia de
/// <c>RegistrarCobroUseCase</c> (ver Fase 2, punto 10) recién resolvió -- si el primer
/// intento en realidad sí llegó a aplicarse del lado del servidor y solo se perdió la
/// respuesta por la red, un reintento automático repetiría la escritura.
/// </summary>
public static class PoliticaReintentoLectura
{
    private static readonly ResiliencePipeline Pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<NpgsqlException>(),
            MaxRetryAttempts = 2,
            Delay = TimeSpan.FromMilliseconds(200),
            BackoffType = DelayBackoffType.Exponential,
        })
        .Build();

    public static async Task<T> EjecutarAsync<T>(
        Func<CancellationToken, Task<T>> operacionDeLectura, CancellationToken cancellationToken) =>
        await Pipeline.ExecuteAsync(
            async ct => await operacionDeLectura(ct), cancellationToken);
}
