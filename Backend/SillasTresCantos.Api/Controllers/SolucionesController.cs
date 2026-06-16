using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/soluciones")]
public class SolucionesController : ControllerBase
{
    private readonly ISolucionService _solucionService;
    private readonly IPermisosCatalogoService _permisosCatalogoService;

    public SolucionesController(ISolucionService solucionService, IPermisosCatalogoService permisosCatalogoService)
    {
        _solucionService = solucionService;
        _permisosCatalogoService = permisosCatalogoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetSolucionDTO>>> GetSoluciones(
        [FromQuery] int idSolucion = 0,
        [FromQuery] string titulo = "",
        [FromQuery] bool orderAsc = true)
    {
        GetSolucionesFiltroDTO filtro = new()
        {
            IdSolucion = idSolucion,
            Titulo = titulo,
            OrderAscent = orderAsc
        };
        List<GetSolucionDTO> soluciones = await _solucionService.GetSolucionesAsync(filtro);
        return Ok(soluciones);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetSolucionDTO>> GetSolucionById(int id)
    {
        GetSolucionDTO? solucion = await _solucionService.GetSolucionByIdAsync(id);
        if (solucion is null)
        {
            return NotFound();
        }

        return Ok(solucion);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<GetSolucionDTO>> PostSolucion([FromBody] PostSolucionDTO solucion)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Solucion, CatalogoPermisoAccion.Crear, HttpContext.RequestAborted))
        {
            return Forbid();
        }

        SolucionOperationResult resultado = await _solucionService.PostSolucionAsync(solucion);
        if (!resultado.IsSuccess)
        {
            return resultado.Error switch
            {
                SolucionOperationError.Validation => BadRequest("Los datos de la solucion no son validos."),
                SolucionOperationError.Conflict => Conflict("Ya existe una solucion con ese titulo."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        if (resultado.Solucion is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return CreatedAtAction(nameof(GetSolucionById), new { id = resultado.Solucion.Id }, resultado.Solucion);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> PutSolucion(int id, [FromBody] PutSolucionDTO solucion)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Solucion, CatalogoPermisoAccion.Modificar, HttpContext.RequestAborted))
        {
            return Forbid();
        }

        solucion.Id = id;
        SolucionOperationResult resultado = await _solucionService.PutSolucionAsync(solucion);

        return resultado.Error switch
        {
            SolucionOperationError.None => NoContent(),
            SolucionOperationError.Validation => BadRequest("Los datos de la solucion no son validos."),
            SolucionOperationError.NotFound => NotFound(),
            SolucionOperationError.Conflict => Conflict("Ya existe una solucion con ese titulo."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteSolucion(int id)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Solucion, CatalogoPermisoAccion.Eliminar, HttpContext.RequestAborted))
        {
            return Forbid();
        }

        SolucionOperationResult resultado = await _solucionService.DeleteSolucionAsync(id);

        return resultado.Error switch
        {
            SolucionOperationError.None => NoContent(),
            SolucionOperationError.Validation => BadRequest("El identificador no es valido."),
            SolucionOperationError.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
