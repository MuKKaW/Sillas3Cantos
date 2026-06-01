using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/integraciones")]
public class IntegracionesController : ControllerBase
{
    private readonly IProductosExternosService _productosExternosService;

    public IntegracionesController(IProductosExternosService productosExternosService)
    {
        _productosExternosService = productosExternosService;
    }

    [HttpGet("productos-externos")]
    public async Task<ActionResult<GetBusquedaProductosExternosDTO>> GetProductosExternos(
        [FromQuery] string query = "",
        [FromQuery] int limit = 10,
        [FromQuery] int skip = 0)
    {
        BuscarProductosExternosFiltroDTO filtro = new()
        {
            Query = query,
            Limit = limit,
            Skip = skip
        };

        ProductosExternosOperationResult resultado = await _productosExternosService
            .BuscarProductosAsync(filtro, HttpContext.RequestAborted);

        if (!resultado.IsSuccess)
        {
            return resultado.Error switch
            {
                ProductosExternosOperationError.Validation => BadRequest("Parametros invalidos: query (2-80), limit (1-30), skip (>=0)."),
                ProductosExternosOperationError.Timeout => StatusCode(StatusCodes.Status504GatewayTimeout, "Timeout al consultar la API externa."),
                ProductosExternosOperationError.Upstream => StatusCode(StatusCodes.Status502BadGateway, "No se pudo obtener una respuesta valida de la API externa."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        if (resultado.Respuesta is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok(resultado.Respuesta);
    }
}
