using FluentValidation;
using FUNBIDE.Application.DTOs.Caja;

namespace FUNBIDE.Application.Validators;

public sealed class AbrirTurnoCajaRequestValidator : AbstractValidator<AbrirTurnoCajaRequest>
{
    public AbrirTurnoCajaRequestValidator()
    {
        RuleFor(x => x.MontoInicial).GreaterThanOrEqualTo(0);
    }
}
