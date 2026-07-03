using FluentValidation;
using FUNBIDE.Application.DTOs.Empleados;

namespace FUNBIDE.Application.Validators;

public sealed class EditarEmpleadoRequestValidator : AbstractValidator<EditarEmpleadoRequest>
{
    public EditarEmpleadoRequestValidator()
    {
        RuleFor(x => x.EmpleadoId).NotEmpty();
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cargo).MaximumLength(100);
    }
}
