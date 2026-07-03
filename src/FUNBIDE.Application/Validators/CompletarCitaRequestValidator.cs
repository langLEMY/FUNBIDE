using FluentValidation;
using FUNBIDE.Application.DTOs.Citas;

namespace FUNBIDE.Application.Validators;

public sealed class CompletarCitaRequestValidator : AbstractValidator<CompletarCitaRequest>
{
    public CompletarCitaRequestValidator()
    {
        RuleFor(x => x.CitaId).NotEmpty();
        RuleFor(x => x.NotasCierre).NotEmpty().MaximumLength(2000);
    }
}
