using FluentValidation;
using FUNBIDE.Application.DTOs.Personal;

namespace FUNBIDE.Application.Validators;

public sealed class CambiarRolRequestValidator : AbstractValidator<CambiarRolRequest>
{
    public CambiarRolRequestValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.NuevoRol).IsInEnum();
    }
}
