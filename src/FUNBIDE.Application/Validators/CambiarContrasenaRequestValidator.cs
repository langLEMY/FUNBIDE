using FluentValidation;
using FUNBIDE.Application.DTOs.Personal;

namespace FUNBIDE.Application.Validators;

public sealed class CambiarContrasenaRequestValidator : AbstractValidator<CambiarContrasenaRequest>
{
    public CambiarContrasenaRequestValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.NuevaContrasena).NotEmpty().MinimumLength(8);
    }
}
