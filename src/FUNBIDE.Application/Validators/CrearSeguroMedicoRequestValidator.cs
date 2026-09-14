using FluentValidation;
using FUNBIDE.Application.DTOs.SegurosMedicos;

namespace FUNBIDE.Application.Validators;

public sealed class CrearSeguroMedicoRequestValidator : AbstractValidator<CrearSeguroMedicoRequest>
{
    public CrearSeguroMedicoRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PorcentajeCobertura!.Value).GreaterThan(0).LessThanOrEqualTo(100)
            .When(x => x.PorcentajeCobertura.HasValue);
    }
}
