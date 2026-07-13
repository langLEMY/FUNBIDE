using FluentValidation;
using FUNBIDE.Application.DTOs.Cobros;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarCobroRequestValidator : AbstractValidator<RegistrarCobroRequest>
{
    public RegistrarCobroRequestValidator()
    {
        RuleFor(x => x.PacienteId).NotEmpty();
        RuleFor(x => x.Concepto).NotEmpty().MaximumLength(300);
        RuleFor(x => x.MontoTotal).GreaterThan(0);
        RuleFor(x => x.MetodoPago).IsInEnum();
        RuleFor(x => x.MontoPagado).GreaterThanOrEqualTo(0);

        When(x => x.SeguroMedicoId.HasValue, () =>
        {
            RuleFor(x => x.CodigoAutorizacion).NotEmpty().MaximumLength(100);
        });
    }
}
