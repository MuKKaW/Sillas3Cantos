using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/marcas")]
public class MarcasController : ControllerBase
{
    private readonly IMarcaService _marcaService;

    public MarcasController(IMarcaService marcaService)
    {
        _marcaService = marcaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetMarcaDTO>>> GetMarcas([FromQuery] int idMarca = 0, [FromQuery] string nombre = "", [FromQuery] bool orderAsc = true)
    {
        GetMarcasFiltroDTO filtro = new()
        {
            IdMarca = idMarca,
            Nombre = nombre,
            OrderAscent = orderAsc
        };
        List<GetMarcaDTO> marcas = await _marcaService.GetMarcasAsync(filtro);
        return Ok(marcas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetMarcaDTO>> GetMarcaById(int id)
    {
        GetMarcaDTO? marca = await _marcaService.GetMarcaByIdAsync(id);
        if (marca is null)
        {
            return NotFound();
        }

        return Ok(marca);
    }

    [HttpPost]
    public async Task<ActionResult<GetMarcaDTO>> PostMarca([FromBody] PostMarcaDTO marca)
    {
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
    public async Task<IActionResult> PutMarca(int id, [FromBody] PutMarcaDTO marca)
    {
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
    public async Task<IActionResult> DeleteMarca(int id)
    {
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
