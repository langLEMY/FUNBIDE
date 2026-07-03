using FluentValidation;
using FUNBIDE.Application.DTOs.Inventario;

namespace FUNBIDE.Application.Validators;

public sealed class DescargarInventarioRequestValidator : AbstractValidator<DescargarInventarioRequest>
{
    public DescargarInventarioRequestValidator()
    {
        RuleFor(x => x.InventarioItemId).NotEmpty();
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.Referencia).MaximumLength(200);
    }
}
