using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface IConfiguracionCatalogoService
{
    Task<GetConfiguracionCatalogoDTO> GetConfiguracionAsync();
    Task<GetConfiguracionCatalogoDTO> PutConfiguracionAsync(PutConfiguracionCatalogoDTO configuracion);
}
