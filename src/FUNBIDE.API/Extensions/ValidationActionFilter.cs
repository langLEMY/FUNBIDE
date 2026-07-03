using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using ValidationException = FUNBIDE.Application.Exceptions.ValidationException;

namespace FUNBIDE.API.Extensions;

/// <summary>
/// Filtro MVC global que ejecuta el <see cref="IValidator{T}"/> registrado (si existe)
/// para cada argumento de la acción antes de invocarla. Evita repetir
/// "validator.Validate(request)" al inicio de cada caso de uso.
/// </summary>
public sealed class ValidationActionFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argumento in context.ActionArguments.Values)
        {
            if (argumento is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argumento.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var resultado = await validator.ValidateAsync(
                new ValidationContext<object>(argumento), context.HttpContext.RequestAborted);

            if (!resultado.IsValid)
            {
                throw new ValidationException(resultado.Errors);
            }
        }

        await next();
    }
}
