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
        RuleFor(x => x.Pagos).NotNull();

        RuleForEach(x => x.Pagos).ChildRules(pago =>
        {
            pago.RuleFor(p => p.Metodo).IsInEnum();
            pago.RuleFor(p => p.Monto).GreaterThan(0);
        });

        RuleFor(x => x.Pagos)
            .Must(pagos => pagos.Select(p => p.Metodo).Distinct().Count() == pagos.Count)
            .WithMessage("No puede haber dos pagos con el mismo método — súmalos en una sola línea.")
            .When(x => x.Pagos is not null);

        When(x => x.SeguroMedicoId.HasValue, () =>
        {
            RuleFor(x => x.CodigoAutorizacion).NotEmpty().MaximumLength(100);
        });
    }
}
