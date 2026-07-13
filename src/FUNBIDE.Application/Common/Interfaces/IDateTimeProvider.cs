namespace FUNBIDE.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Zona horaria de la clínica (República Dominicana, UTC-4 fijo, sin horario de
    /// verano). Los cortes de "día" para Agenda/Sala de Espera deben calcularse con esta
    /// zona, no en UTC: de lo contrario una cita de la tarde/noche local cae en el día
    /// UTC siguiente y desaparece de "hoy".
    /// </summary>
    TimeZoneInfo ZonaHorariaClinica { get; }
}
