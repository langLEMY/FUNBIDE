using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Auth;
using FUNBIDE.Domain.Exceptions;

namespace FUNBIDE.Application.UseCases.Auth;

public interface IIniciarSesionLocalUseCase : IUseCase<IniciarSesionLocalRequest, IniciarSesionLocalResultDto>
{
}

/// <summary>
/// Login del modo Auth:Provider=Local (sin Supabase): verifica la contraseña contra
/// la credencial local y emite un JWT firmado por esta misma API. Solo tiene sentido
/// invocado cuando Auth:Provider=Local (ver <c>DependencyInjection</c>).
/// </summary>
public sealed class IniciarSesionLocalUseCase(
    IAutenticacionLocalService autenticacionLocal) : IIniciarSesionLocalUseCase
{
    public async Task<IniciarSesionLocalResultDto> EjecutarAsync(
        IniciarSesionLocalRequest request, CancellationToken cancellationToken)
    {
        var resultado = await autenticacionLocal.IniciarSesionAsync(
            request.Correo, request.Contrasena, cancellationToken);

        if (resultado is null)
        {
            throw new CredencialesInvalidasException();
        }

        return new IniciarSesionLocalResultDto(resultado.AccessToken, resultado.ExpiraEn);
    }
}
