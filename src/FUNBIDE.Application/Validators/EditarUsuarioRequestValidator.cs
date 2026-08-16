using FluentValidation;
using FUNBIDE.Application.DTOs.Personal;

namespace FUNBIDE.Application.Validators;

public sealed class EditarUsuarioRequestValidator : AbstractValidator<EditarUsuarioRequest>
{
    public EditarUsuarioRequestValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Correo).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.NombreUsuario).NotEmpty().MinimumLength(3).MaximumLength(50)
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("El nombre de usuario solo puede tener letras, números, puntos, guiones y guiones bajos.");
    }
}
