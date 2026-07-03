using FluentValidation;
using FUNBIDE.Application.DTOs.Empleados;

namespace FUNBIDE.Application.Validators;

public sealed class CrearEmpleadoRequestValidator : AbstractValidator<CrearEmpleadoRequest>
{
    public CrearEmpleadoRequestValidator()
    {
        RuleFor(x => x.NombreCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cargo).MaximumLength(100);
    }
}
