using FluentValidation;
using FUNBIDE.Application.DTOs.Permisos;

namespace FUNBIDE.Application.Validators;

public sealed class ActualizarPermisosDeUsuarioRequestValidator : AbstractValidator<ActualizarPermisosDeUsuarioRequest>
{
    public ActualizarPermisosDeUsuarioRequestValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.Permisos).NotNull();
        RuleForEach(x => x.Permisos).ChildRules(permiso =>
        {
            permiso.RuleFor(p => p.Modulo).IsInEnum();
        });
    }
}
