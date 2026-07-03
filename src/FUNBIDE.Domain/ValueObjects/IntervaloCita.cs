namespace FUNBIDE.Domain.ValueObjects;

/// <summary>
/// Representa el intervalo de tiempo programado para una cita médica.
/// Garantiza invariantes de coherencia temporal en el punto de construcción.
/// </summary>
public sealed record IntervaloCita
{
    public DateTimeOffset Inicio { get; }
    public DateTimeOffset Fin { get; }

    private IntervaloCita(DateTimeOffset inicio, DateTimeOffset fin)
    {
        Inicio = inicio;
        Fin = fin;
    }

    public static IntervaloCita Crear(DateTimeOffset inicio, DateTimeOffset fin)
    {
        if (fin <= inicio)
        {
            throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio.", nameof(fin));
        }

        return new IntervaloCita(inicio, fin);
    }

    public TimeSpan Duracion => Fin - Inicio;
}
