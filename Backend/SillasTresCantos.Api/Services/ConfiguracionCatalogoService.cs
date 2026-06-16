using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class ConfiguracionCatalogoService : IConfiguracionCatalogoService
{
    private readonly IConfiguracionCatalogoRepository _configuracionRepository;

    public ConfiguracionCatalogoService(IConfiguracionCatalogoRepository configuracionRepository)
    {
        _configuracionRepository = configuracionRepository;
    }

    public async Task<GetConfiguracionCatalogoDTO> GetConfiguracionAsync()
    {
        ConfiguracionCatalogo configuracion = await _configuracionRepository.GetConfiguracionAsync();
        return MapToGetConfiguracionCatalogoDTO(configuracion);
    }

    public async Task<GetConfiguracionCatalogoDTO> PutConfiguracionAsync(PutConfiguracionCatalogoDTO configuracion)
    {
        ConfiguracionCatalogo actualizada = await _configuracionRepository.UpdateConfiguracionAsync(new ConfiguracionCatalogo
        {
            UsarFiltroTabs = configuracion.UsarFiltroTabs,
            MostrarPrecios = configuracion.MostrarPrecios,
            MostrarStock = configuracion.MostrarStock,
            FechaActualizacion = DateTime.UtcNow
        });

        return MapToGetConfiguracionCatalogoDTO(actualizada);
    }

    private static GetConfiguracionCatalogoDTO MapToGetConfiguracionCatalogoDTO(ConfiguracionCatalogo configuracion) =>
        new()
        {
            UsarFiltroTabs = configuracion.UsarFiltroTabs,
            MostrarPrecios = configuracion.MostrarPrecios,
            MostrarStock = configuracion.MostrarStock,
            FechaActualizacion = configuracion.FechaActualizacion
        };
}
