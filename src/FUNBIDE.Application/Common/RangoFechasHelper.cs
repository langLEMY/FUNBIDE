namespace FUNBIDE.Application.Common;

/// <summary>
/// Recorta un rango de fechas a un span máximo razonable. Pensado para reportes cuyo
/// Desde/Hasta ya es obligatorio en el request ([ApiController] rechaza con 400 si falta
/// uno de los dos, así que no hace falta un default como en ObtenerLogsAuditoriaUseCase),
/// pero que igual quedan expuestos a un rango absurdamente largo (ej. "desde el año 1")
/// que sería costoso de calcular sin aportar nada útil.
/// </summary>
public static class RangoFechasHelper
{
    public const int SpanMaximoDiasPorDefecto = 366;

    public static (DateTimeOffset Desde, DateTimeOffset Hasta) RecortarASpanMaximo(
        DateTimeOffset desde, DateTimeOffset hasta, int spanMaximoDias = SpanMaximoDiasPorDefecto)
    {
        if (hasta - desde > TimeSpan.FromDays(spanMaximoDias))
        {
            desde = hasta - TimeSpan.FromDays(spanMaximoDias);
        }

        return (desde, hasta);
    }
}
