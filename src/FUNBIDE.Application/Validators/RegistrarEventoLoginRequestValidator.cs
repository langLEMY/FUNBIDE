using FluentValidation;
using FUNBIDE.Application.DTOs.Auth;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarEventoLoginRequestValidator : AbstractValidator<RegistrarEventoLoginRequest>
{
    public RegistrarEventoLoginRequestValidator()
    {
        RuleFor(x => x.Correo).NotEmpty().MaximumLength(320).EmailAddress();
    }
}
