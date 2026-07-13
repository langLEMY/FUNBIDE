using FluentValidation;
using FUNBIDE.Application.DTOs.Citas;

namespace FUNBIDE.Application.Validators;

public sealed class AgendarCitaRequestValidator : AbstractValidator<AgendarCitaRequest>
{
    public AgendarCitaRequestValidator()
    {
        RuleFor(x => x.PacienteId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Fin).GreaterThan(x => x.Inicio);
        RuleFor(x => x.Inicio).GreaterThan(DateTimeOffset.UtcNow).WithMessage("La cita debe agendarse en el futuro.");
    }
}
