using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/marcas")]
public class MarcasController : ControllerBase
{
    private readonly IMarcaService _marcaService;
    private readonly IPermisosCatalogoService _permisosCatalogoService;

    public MarcasController(IMarcaService marcaService, IPermisosCatalogoService permisosCatalogoService)
    {
        _marcaService = marcaService;
        _permisosCatalogoService = permisosCatalogoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetMarcaDTO>>> GetMarcas(
        [FromQuery] int idMarca = 0,
        [FromQuery] string nombre = "",
        [FromQuery] bool orderAsc = true,
        [FromQuery] bool includeHidden = false)
    {
        if (includeHidden && !(User.Identity?.IsAuthenticated ?? false))
        {
            return Unauthorized("Debes autenticarte para consultar marcas ocultas.");
        }

        GetMarcasFiltroDTO filtro = new()
        {
            IdMarca = idMarca,
            Nombre = nombre,
            OrderAscent = orderAsc,
            IncludeHidden = includeHidden
        };
        List<GetMarcaDTO> marcas = await _marcaService.GetMarcasAsync(filtro);
        return Ok(marcas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetMarcaDTO>> GetMarcaById(int id, [FromQuery] bool includeHidden = false)
    {
        if (includeHidden && !(User.Identity?.IsAuthenticated ?? false))
        {
            return Unauthorized("Debes autenticarte para consultar marcas ocultas.");
        }

        GetMarcaDTO? marca = await _marcaService.GetMarcaByIdAsync(id);
        if (marca is null)
        {
            return NotFound();
        }

        if (!includeHidden && !marca.EsVisible)
        {
            return NotFound();
        }

        return Ok(marca);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<GetMarcaDTO>> PostMarca([FromBody] PostMarcaDTO marca)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Marca, CatalogoPermisoAccion.Crear, HttpContext.RequestAborted))
        {
            return Forbid();
        }

        MarcaOperationResult resultado = await _marcaService.PostMarcaAsync(marca);
        if (!resultado.IsSuccess)
        {
            return resultado.Error switch
            {
                MarcaOperationError.Validation => BadRequest("Los datos de la marca no son validos."),
                MarcaOperationError.Conflict => Conflict("Ya existe una marca con ese nombre."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        if (resultado.Marca is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return CreatedAtAction(nameof(GetMarcaById), new { id = resultado.Marca.Id }, resultado.Marca);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> PutMarca(int id, [FromBody] PutMarcaDTO marca)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Marca, CatalogoPermisoAccion.Modificar, HttpContext.RequestAborted))
        {
            return Forbid();
        }

        marca.Id = id;
        MarcaOperationResult resultado = await _marcaService.PutMarcaAsync(marca);

        return resultado.Error switch
        {
            MarcaOperationError.None => NoContent(),
            MarcaOperationError.Validation => BadRequest("Los datos de la marca no son validos."),
            MarcaOperationError.NotFound => NotFound(),
            MarcaOperationError.Conflict => Conflict("Ya existe una marca con ese nombre."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMarca(int id)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Marca, CatalogoPermisoAccion.Eliminar, HttpContext.RequestAborted))
        {
            return Forbid();
        }

        MarcaOperationResult resultado = await _marcaService.DeleteMarcaAsync(id);

        return resultado.Error switch
        {
            MarcaOperationError.None => NoContent(),
            MarcaOperationError.Validation => BadRequest("El identificador no es valido."),
            MarcaOperationError.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
