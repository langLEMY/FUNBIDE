using FluentValidation;
using FUNBIDE.Application.DTOs.Sesiones;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarLatidoRequestValidator : AbstractValidator<RegistrarLatidoRequest>
{
    public RegistrarLatidoRequestValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().MaximumLength(100);
    }
}
