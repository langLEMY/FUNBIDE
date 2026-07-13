using FluentValidation;
using FUNBIDE.Application.DTOs.Pacientes;

namespace FUNBIDE.Application.Validators;

public sealed class CrearPacienteRequestValidator : AbstractValidator<CrearPacienteRequest>
{
    public CrearPacienteRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Cedula).NotEmpty().Length(5, 20);
        RuleFor(x => x.Telefono).MaximumLength(30);
        RuleFor(x => x.Edad).InclusiveBetween(0, 130).When(x => x.Edad.HasValue);
        RuleFor(x => x.Condicion).MaximumLength(200);
    }
}
