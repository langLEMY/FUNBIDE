using FluentValidation;
using FUNBIDE.Application.DTOs.Citas;

namespace FUNBIDE.Application.Validators;

public sealed class CrearCitaRequestValidator : AbstractValidator<CrearCitaRequest>
{
    public CrearCitaRequestValidator()
    {
        RuleFor(x => x.PacienteId).NotEmpty();
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(500);
    }
}
