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
    }
}
