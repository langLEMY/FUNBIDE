using FluentValidation;
using FUNBIDE.Application.DTOs.Inventario;

namespace FUNBIDE.Application.Validators;

public sealed class CrearInventarioItemRequestValidator : AbstractValidator<CrearInventarioItemRequest>
{
    public CrearInventarioItemRequestValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.StockInicial).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockMinimo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Categoria).IsInEnum();
    }
}
