using FluentValidation;
using FUNBIDE.Application.DTOs.SegurosMedicos;

namespace FUNBIDE.Application.Validators;

public sealed class EditarTarifarioProcedimientoRequestValidator : AbstractValidator<EditarTarifarioProcedimientoRequest>
{
    public EditarTarifarioProcedimientoRequestValidator()
    {
        RuleFor(x => x.TarifarioProcedimientoId).NotEmpty();
        RuleFor(x => x.MontoSeguro).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MontoPaciente).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MontoTotal).GreaterThan(0);
        RuleFor(x => x.MontoFondo).GreaterThanOrEqualTo(0).When(x => x.MontoFondo.HasValue);
    }
}
