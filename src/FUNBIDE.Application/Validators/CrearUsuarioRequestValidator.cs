using FluentValidation;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.Validators;

public sealed class CrearUsuarioRequestValidator : AbstractValidator<CrearUsuarioRequest>
{
    public CrearUsuarioRequestValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Correo).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.NombreUsuario).NotEmpty().MinimumLength(3).MaximumLength(50)
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("El nombre de usuario solo puede tener letras, números, puntos, guiones y guiones bajos.");
        RuleFor(x => x.ContrasenaTemporal).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Rol).IsInEnum();

        RuleFor(x => x.Especialidad)
            .NotNull().When(x => x.Rol == RolUsuario.Doctor)
            .WithMessage("La especialidad es obligatoria para un usuario con rol Doctor.");
        RuleFor(x => x.Especialidad)
            .IsInEnum().When(x => x.Especialidad is not null);
        RuleFor(x => x.Especialidad)
            .Null().When(x => x.Rol != RolUsuario.Doctor)
            .WithMessage("Solo un usuario con rol Doctor puede tener una especialidad.");
    }
}
