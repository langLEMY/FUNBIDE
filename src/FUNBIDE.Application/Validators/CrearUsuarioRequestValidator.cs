using FluentValidation;
using FUNBIDE.Application.DTOs.Personal;

namespace FUNBIDE.Application.Validators;

public sealed class CrearUsuarioRequestValidator : AbstractValidator<CrearUsuarioRequest>
{
    public CrearUsuarioRequestValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Correo).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.ContrasenaTemporal).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Rol).IsInEnum();
    }
}
