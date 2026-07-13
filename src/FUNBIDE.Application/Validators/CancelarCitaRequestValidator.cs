using FluentValidation;
using FUNBIDE.Application.DTOs.Citas;

namespace FUNBIDE.Application.Validators;

public sealed class CancelarCitaRequestValidator : AbstractValidator<CancelarCitaRequest>
{
    public CancelarCitaRequestValidator()
    {
        RuleFor(x => x.CitaId).NotEmpty();
    }
}
