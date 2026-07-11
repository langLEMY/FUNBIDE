using FluentValidation;
using FUNBIDE.Application.DTOs.Auth;

namespace FUNBIDE.Application.Validators;

public sealed class IniciarSesionLocalRequestValidator : AbstractValidator<IniciarSesionLocalRequest>
{
    public IniciarSesionLocalRequestValidator()
    {
        RuleFor(x => x.Correo).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Contrasena).NotEmpty().MaximumLength(200);
    }
}
