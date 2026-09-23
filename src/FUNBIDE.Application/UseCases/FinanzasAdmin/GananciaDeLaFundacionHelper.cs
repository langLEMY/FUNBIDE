namespace FUNBIDE.Application.UseCases.FinanzasAdmin;

/// <summary>
/// Compartido por <see cref="ObtenerResumenAnualUseCase"/> y <see cref="ObtenerResumenPorPeriodoUseCase"/>:
/// un ingreso manual (Caja/Finanzas) marcado por su concepto como "ganancia de la fundación"
/// también se refleja en el desglose de Fondo — ver CajaPage.tsx, que antepone esta frase al
/// concepto cuando el cajero marca el checkbox "Es ganancia de la fundación".
/// </summary>
internal static class GananciaDeLaFundacionHelper
{
    public static bool EsGananciaDeLaFundacion(string concepto) =>
        concepto.Contains("ganancia de la fundación", StringComparison.OrdinalIgnoreCase) ||
        concepto.Contains("ganancia fundación", StringComparison.OrdinalIgnoreCase) ||
        concepto.Contains("ganancia de la fundacion", StringComparison.OrdinalIgnoreCase);
}
