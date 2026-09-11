namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Qué es una entrada del historial clínico (ver <see cref="Entities.EntradaHistorialClinico"/>):
/// desde la nota libre de siempre (<see cref="NotaClinica"/>) hasta los documentos formales que
/// el paciente se lleva impresos. Los cinco <c>Receta*</c> comparten la misma forma de contenido
/// (lista de ítems con indicaciones — ver <c>ContenidoReceta</c> en el frontend); los seis
/// restantes son un cuerpo de texto libre con membrete formal (ver <c>ContenidoDocumento</c>).
/// Se guarda como string en la base (ver <c>EntradaHistorialClinicoConfiguration</c>), así que
/// agregar un valor nuevo acá no requiere migración.
/// </summary>
public enum TipoEntradaHistorial
{
    NotaClinica,
    RecetaMedicamento,
    RecetaImagenes,
    RecetaAnaliticas,
    RecetaVacunas,
    RecetaProcedimientos,
    LicenciaMedica,
    Prequirurgica,
    ConstanciaConsulta,
    Referimiento,
    OrdenMedica,
    EvolucionMedica,
}
