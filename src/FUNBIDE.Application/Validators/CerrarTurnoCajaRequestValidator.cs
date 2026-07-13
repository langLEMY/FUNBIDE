using FluentValidation;
using FUNBIDE.Application.DTOs.Caja;

namespace FUNBIDE.Application.Validators;

public sealed class CerrarTurnoCajaRequestValidator : AbstractValidator<CerrarTurnoCajaRequest>
{
    public CerrarTurnoCajaRequestValidator()
    {
        RuleFor(x => x.MontoFinalContado).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notas).MaximumLength(2000);
    }
}
