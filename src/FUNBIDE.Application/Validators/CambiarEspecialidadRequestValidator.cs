using FluentValidation;
using FUNBIDE.Application.DTOs.Personal;

namespace FUNBIDE.Application.Validators;

public sealed class CambiarEspecialidadRequestValidator : AbstractValidator<CambiarEspecialidadRequest>
{
    public CambiarEspecialidadRequestValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.Especialidad).IsInEnum();
    }
}
