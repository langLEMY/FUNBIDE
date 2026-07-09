using FluentValidation;
using FUNBIDE.Application.DTOs.Inventario;

namespace FUNBIDE.Application.Validators;

public sealed class EditarInventarioItemRequestValidator : AbstractValidator<EditarInventarioItemRequest>
{
    public EditarInventarioItemRequestValidator()
    {
        RuleFor(x => x.InventarioItemId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.StockActual).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockMinimo).GreaterThanOrEqualTo(0);
    }
}
