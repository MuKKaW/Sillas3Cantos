using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/configuracion/catalogo")]
public class ConfiguracionCatalogoController : ControllerBase
{
    private readonly IConfiguracionCatalogoService _configuracionService;

    public ConfiguracionCatalogoController(IConfiguracionCatalogoService configuracionService)
    {
        _configuracionService = configuracionService;
    }

    [HttpGet]
    public async Task<ActionResult<GetConfiguracionCatalogoDTO>> GetConfiguracion()
    {
        GetConfiguracionCatalogoDTO configuracion = await _configuracionService.GetConfiguracionAsync();
        return Ok(configuracion);
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<GetConfiguracionCatalogoDTO>> PutConfiguracion(
        [FromBody] PutConfiguracionCatalogoDTO configuracion)
    {
        GetConfiguracionCatalogoDTO actualizada = await _configuracionService.PutConfiguracionAsync(configuracion);
        return Ok(actualizada);
    }
}
