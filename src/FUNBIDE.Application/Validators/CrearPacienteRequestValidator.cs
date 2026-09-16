using FluentValidation;
using FUNBIDE.Application.DTOs.Pacientes;

namespace FUNBIDE.Application.Validators;

public sealed class CrearPacienteRequestValidator : AbstractValidator<CrearPacienteRequest>
{
    public CrearPacienteRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede tener más de 150 caracteres.");
        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(150).WithMessage("El apellido no puede tener más de 150 caracteres.");
        RuleFor(x => x.Cedula)
            .NotEmpty().WithMessage("La cédula es obligatoria.")
            .Length(5, 20).WithMessage("La cédula debe tener entre 5 y 20 caracteres.");
        RuleFor(x => x.Telefono).MaximumLength(30).WithMessage("El teléfono no puede tener más de 30 caracteres.");
        RuleFor(x => x.Edad)
            .InclusiveBetween(0, 130).WithMessage("La edad debe estar entre 0 y 130 años.")
            .When(x => x.Edad.HasValue);
        RuleFor(x => x.Condicion).MaximumLength(200).WithMessage("La condición no puede tener más de 200 caracteres.");
    }
}
