using FluentValidation;
using FUNBIDE.Application.DTOs.Finanzas;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarMovimientoFinancieroRequestValidator : AbstractValidator<RegistrarMovimientoFinancieroRequest>
{
    public RegistrarMovimientoFinancieroRequestValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Monto).GreaterThan(0);
        RuleFor(x => x.Concepto).NotEmpty().MaximumLength(300);
    }
}
