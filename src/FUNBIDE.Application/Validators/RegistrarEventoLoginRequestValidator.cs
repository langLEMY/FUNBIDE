using FluentValidation;
using FUNBIDE.Application.DTOs.Auth;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarEventoLoginRequestValidator : AbstractValidator<RegistrarEventoLoginRequest>
{
    public RegistrarEventoLoginRequestValidator()
    {
        RuleFor(x => x.NombreUsuario).NotEmpty().MaximumLength(320);
    }
}
