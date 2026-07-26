using FluentValidation;
using FUNBIDE.Application.DTOs.Donaciones;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarDonacionRequestValidator : AbstractValidator<RegistrarDonacionRequest>
{
    public RegistrarDonacionRequestValidator()
    {
        RuleFor(x => x.DonanteNombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DonanteContacto).MaximumLength(200);
        RuleFor(x => x.Monto).GreaterThan(0);
        RuleFor(x => x.Concepto).NotEmpty().MaximumLength(300);
    }
}
