using FluentValidation;
using FUNBIDE.Application.DTOs.FinanzasAdmin;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarGastoAdminRequestValidator : AbstractValidator<RegistrarGastoAdminRequest>
{
    public RegistrarGastoAdminRequestValidator()
    {
        RuleFor(x => x.Concepto).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Monto).GreaterThan(0);
    }
}
