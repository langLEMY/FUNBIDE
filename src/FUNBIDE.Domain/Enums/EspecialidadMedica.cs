namespace FUNBIDE.Domain.Enums;

/// <summary>
/// Especialidades médicas ofrecidas por FUNBIDE (ver sello institucional). Solo aplica
/// a usuarios con <see cref="RolUsuario.Doctor"/> — ver <see cref="Entities.Usuario.AsignarEspecialidad"/>.
/// </summary>
public enum EspecialidadMedica
{
    Sonografia,
    Odontologia,
    Pediatria,
    Cardiologia,
    Ginecologia,
    MedicinaGeneralYFamiliar,
    Diabetologia,
    Psicologia,
    Oftalmologia,
    MedicinaInterna,
    Ortopedia,
    Nutricion
}
