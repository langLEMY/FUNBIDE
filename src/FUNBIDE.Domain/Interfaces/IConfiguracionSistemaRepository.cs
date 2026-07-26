using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IConfiguracionSistemaRepository
{
    /// <summary><c>null</c> si nadie tocó nunca la configuración — equivale a todo desactivado.</summary>
    Task<ConfiguracionSistema?> ObtenerAsync(CancellationToken cancellationToken);

    Task AgregarAsync(ConfiguracionSistema configuracion, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
