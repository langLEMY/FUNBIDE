using FluentValidation;
using FUNBIDE.Application.DTOs.Sistema;

namespace FUNBIDE.Application.Validators;

public sealed class CambiarModoMantenimientoRequestValidator : AbstractValidator<CambiarModoMantenimientoRequest>
{
    public CambiarModoMantenimientoRequestValidator()
    {
        RuleFor(x => x.Mensaje).MaximumLength(500);
    }
}
