using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public interface IConfiguracionCatalogoRepository
{
    Task<ConfiguracionCatalogo> GetConfiguracionAsync(CancellationToken cancellationToken = default);
    Task<ConfiguracionCatalogo> UpdateConfiguracionAsync(ConfiguracionCatalogo configuracion, CancellationToken cancellationToken = default);
}
