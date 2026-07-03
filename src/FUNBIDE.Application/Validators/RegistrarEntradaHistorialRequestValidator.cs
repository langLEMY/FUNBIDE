using FluentValidation;
using FUNBIDE.Application.DTOs.HistorialClinico;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarEntradaHistorialRequestValidator : AbstractValidator<RegistrarEntradaHistorialRequest>
{
    public RegistrarEntradaHistorialRequestValidator()
    {
        RuleFor(x => x.PacienteId).NotEmpty();
        RuleFor(x => x.Contenido)
            .Must(c => c.ValueKind == System.Text.Json.JsonValueKind.Object)
            .WithMessage("El contenido del historial clínico debe ser un objeto JSON.");
    }
}
