using FluentValidation;
using FUNBIDE.Application.DTOs.Citas;

namespace FUNBIDE.Application.Validators;

public sealed class ProgramarCitaRequestValidator : AbstractValidator<ProgramarCitaRequest>
{
    public ProgramarCitaRequestValidator()
    {
        RuleFor(x => x.CitaId).NotEmpty();
        RuleFor(x => x.Fin).GreaterThan(x => x.Inicio);
        RuleFor(x => x.Inicio).GreaterThan(DateTimeOffset.UtcNow).WithMessage("La cita debe programarse en el futuro.");
    }
}
