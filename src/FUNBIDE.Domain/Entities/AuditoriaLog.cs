using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.ValueObjects;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Registro inmutable de auditoría (una petición HTTP, un evento de negocio, etc.).
/// Append-only: se inserta y se lee, nunca se modifica.
/// </summary>
public sealed class AuditoriaLog : AppendOnlyEntity
{
    public Guid? UsuarioId { get; private set; }
    public string Accion { get; private set; } = string.Empty;
    public string Recurso { get; private set; } = string.Empty;
    public int? CodigoRespuestaHttp { get; private set; }
    public DocumentoJson Detalle { get; private set; } = null!;

    private AuditoriaLog() { }

    public AuditoriaLog(string accion, string recurso, DocumentoJson detalle, Guid? usuarioId, int? codigoRespuestaHttp)
    {
        if (string.IsNullOrWhiteSpace(accion))
        {
            throw new ArgumentException("La acción auditada es obligatoria.", nameof(accion));
        }

        if (string.IsNullOrWhiteSpace(recurso))
        {
            throw new ArgumentException("El recurso auditado es obligatorio.", nameof(recurso));
        }

        UsuarioId = usuarioId;
        Accion = accion.Trim();
        Recurso = recurso.Trim();
        Detalle = detalle;
        CodigoRespuestaHttp = codigoRespuestaHttp;
    }
}
