namespace FUNBIDE.Domain.Enums;

public enum EstadoCita
{
    Pendiente,
    Programada,

    /// <summary>El paciente ya llegó y está físicamente esperando a ser atendido.</summary>
    EnEspera,

    Completada,
    Cancelada
}
