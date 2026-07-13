using FluentValidation;
using FUNBIDE.Application.DTOs.Personal;

namespace FUNBIDE.Application.Validators;

public sealed class ActualizarMiPerfilRequestValidator : AbstractValidator<ActualizarMiPerfilRequest>
{
    public ActualizarMiPerfilRequestValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Correo).NotEmpty().EmailAddress().MaximumLength(320);
    }
}
