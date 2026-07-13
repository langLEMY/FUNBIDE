using FluentValidation;
using FUNBIDE.Application.DTOs.SegurosMedicos;

namespace FUNBIDE.Application.Validators;

public sealed class EditarSeguroMedicoRequestValidator : AbstractValidator<EditarSeguroMedicoRequest>
{
    public EditarSeguroMedicoRequestValidator()
    {
        RuleFor(x => x.SeguroMedicoId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PorcentajeCobertura).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
