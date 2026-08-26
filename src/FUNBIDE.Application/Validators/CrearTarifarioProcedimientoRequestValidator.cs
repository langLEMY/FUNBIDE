using FluentValidation;
using FUNBIDE.Application.DTOs.SegurosMedicos;

namespace FUNBIDE.Application.Validators;

public sealed class CrearTarifarioProcedimientoRequestValidator : AbstractValidator<CrearTarifarioProcedimientoRequest>
{
    public CrearTarifarioProcedimientoRequestValidator()
    {
        RuleFor(x => x.SeguroMedicoId).NotEmpty();
        RuleFor(x => x.Procedimiento).NotEmpty().MaximumLength(300);
        RuleFor(x => x.MontoSeguro).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MontoPaciente).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MontoTotal).GreaterThan(0);
        RuleFor(x => x.MontoFondo).GreaterThanOrEqualTo(0).When(x => x.MontoFondo.HasValue);
    }
}
