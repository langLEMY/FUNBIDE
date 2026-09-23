using FluentValidation;
using FUNBIDE.Application.DTOs.Inventario;

namespace FUNBIDE.Application.Validators;

public sealed class RegistrarEntradaInventarioRequestValidator : AbstractValidator<RegistrarEntradaInventarioRequest>
{
    public RegistrarEntradaInventarioRequestValidator()
    {
        RuleFor(x => x.InventarioItemId).NotEmpty();
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.Referencia).MaximumLength(200);
    }
}
