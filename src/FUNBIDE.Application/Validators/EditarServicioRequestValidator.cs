using FluentValidation;
using FUNBIDE.Application.DTOs.Servicios;

namespace FUNBIDE.Application.Validators;

public sealed class EditarServicioRequestValidator : AbstractValidator<EditarServicioRequest>
{
    public EditarServicioRequestValidator()
    {
        RuleFor(x => x.ServicioId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Precio1).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Precio2).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Precio3).GreaterThanOrEqualTo(0);
    }
}
