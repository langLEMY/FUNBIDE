using FluentValidation;
using FUNBIDE.Application.DTOs.Citas;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarLlegadaSinCitaRequestValidator : AbstractValidator<RegistrarLlegadaSinCitaRequest>
{
    public RegistrarLlegadaSinCitaRequestValidator()
    {
        RuleFor(x => x.PacienteId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(500);
    }
}
