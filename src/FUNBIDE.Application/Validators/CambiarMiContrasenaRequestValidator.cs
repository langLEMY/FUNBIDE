using FluentValidation;
using FUNBIDE.Application.DTOs.Personal;

namespace FUNBIDE.Application.Validators;

public sealed class CambiarMiContrasenaRequestValidator : AbstractValidator<CambiarMiContrasenaRequest>
{
    public CambiarMiContrasenaRequestValidator()
    {
        RuleFor(x => x.NuevaContrasena).NotEmpty().MinimumLength(8).MaximumLength(200);
    }
}
