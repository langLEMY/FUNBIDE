using FluentValidation;
using FUNBIDE.Application.DTOs.Citas;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarLlegadaRequestValidator : AbstractValidator<RegistrarLlegadaRequest>
{
    public RegistrarLlegadaRequestValidator()
    {
        RuleFor(x => x.CitaId).NotEmpty();
    }
}
