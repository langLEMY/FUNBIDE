using FluentValidation;
using FUNBIDE.Application.DTOs.Cobros;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarCobroRequestValidator : AbstractValidator<RegistrarCobroRequest>
{
    public RegistrarCobroRequestValidator()
    {
        RuleFor(x => x.PacienteId).NotEmpty().WithMessage("Selecciona un paciente.");
        RuleFor(x => x.Concepto)
            .NotEmpty().WithMessage("El concepto del cobro es obligatorio.")
            .MaximumLength(300).WithMessage("El concepto no puede tener más de 300 caracteres.");
        RuleFor(x => x.MontoTotal).GreaterThan(0).WithMessage("El monto total debe ser mayor a cero.");
        RuleFor(x => x.Pagos).NotNull().WithMessage("Agrega al menos una forma de pago.");

        RuleForEach(x => x.Pagos).ChildRules(pago =>
        {
            pago.RuleFor(p => p.Metodo).IsInEnum().WithMessage("El método de pago no es válido.");
            pago.RuleFor(p => p.Monto).GreaterThan(0).WithMessage("El monto de cada pago debe ser mayor a cero.");
        });

        RuleFor(x => x.Pagos)
            .Must(pagos => pagos.Select(p => p.Metodo).Distinct().Count() == pagos.Count)
            .WithMessage("No puede haber dos pagos con el mismo método — súmalos en una sola línea.")
            .When(x => x.Pagos is not null);

        // El cálculo automático por % de cobertura quedó desactivado: todo cobro con
        // seguro médico tiene que traer un procedimiento del tarifario, que es lo único
        // que fija el monto que cubre la aseguradora (ver RegistrarCobroUseCase).
        When(x => x.SeguroMedicoId.HasValue, () =>
        {
            RuleFor(x => x.CodigoAutorizacion)
                .NotEmpty().WithMessage("El código de autorización de la aseguradora es obligatorio.")
                .MaximumLength(100).WithMessage("El código de autorización no puede tener más de 100 caracteres.");
            RuleFor(x => x.TarifarioProcedimientoId)
                .NotEmpty()
                .WithMessage("Selecciona un procedimiento del tarifario — el cálculo automático por % de cobertura está desactivado.");
        });
    }
}
