using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;
    private readonly IPermisosCatalogoService _permisosCatalogoService;

    public CategoriasController(ICategoriaService categoriaService, IPermisosCatalogoService permisosCatalogoService)
    {
        _categoriaService = categoriaService;
        _permisosCatalogoService = permisosCatalogoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetCategoriaDTO>>> GetCategorias(
        [FromQuery] int idCategoria = 0,
        [FromQuery] string nombre = "",
        [FromQuery] bool orderAsc = true,
        [FromQuery] bool includeHidden = false)
    {
        if (includeHidden && !(User.Identity?.IsAuthenticated ?? false))
        {
            return Unauthorized("Debes autenticarte para consultar categorias ocultas.");
        }

        GetCategoriasFiltroDTO filtro = new()
        {
            IdCategoria = idCategoria,
            Nombre = nombre,
            OrderAscent = orderAsc,
            IncludeHidden = includeHidden
        };
        List<GetCategoriaDTO> categorias = await _categoriaService.GetCategoriasAsync(filtro);
        return Ok(categorias);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetCategoriaDTO>> GetCategoriaById(int id, [FromQuery] bool includeHidden = false)
    {
        if (includeHidden && !(User.Identity?.IsAuthenticated ?? false))
        {
            return Unauthorized("Debes autenticarte para consultar categorias ocultas.");
        }

        GetCategoriaDTO? categoria = await _categoriaService.GetCategoriaByIdAsync(id);
        if (categoria is null)
        {
            return NotFound();
        }

        if (!includeHidden && !categoria.EsVisible)
        {
            return NotFound();
        }

        return Ok(categoria);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<GetCategoriaDTO>> PostCategoria([FromBody] PostCategoriaDTO categoria)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Categoria, CatalogoPermisoAccion.Crear, HttpContext.RequestAborted))
        {
            return Forbid();
        }

        CategoriaOperationResult resultado = await _categoriaService.PostCategoriaAsync(categoria);
        if (!resultado.IsSuccess)
        {
            return resultado.Error switch
            {
                CategoriaOperationError.Validation => BadRequest("Los datos de la categoria no son validos."),
                CategoriaOperationError.Conflict => Conflict("Ya existe una categoria con ese nombre."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        if (resultado.Categoria is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return CreatedAtAction(nameof(GetCategoriaById), new { id = resultado.Categoria.Id }, resultado.Categoria);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> PutCategoria(int id, [FromBody] PutCategoriaDTO categoria)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Categoria, CatalogoPermisoAccion.Modificar, HttpContext.RequestAborted))
        {
            return Forbid();
        }

        categoria.Id = id;
        CategoriaOperationResult resultado = await _categoriaService.PutCategoriaAsync(categoria);

        return resultado.Error switch
        {
            CategoriaOperationError.None => NoContent(),
            CategoriaOperationError.Validation => BadRequest("Los datos de la categoria no son validos."),
            CategoriaOperationError.NotFound => NotFound(),
            CategoriaOperationError.Conflict => Conflict("Ya existe una categoria con ese nombre."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Categoria, CatalogoPermisoAccion.Eliminar, HttpContext.RequestAborted))
        {
            return Forbid();
        }

        CategoriaOperationResult resultado = await _categoriaService.DeleteCategoriaAsync(id);

        return resultado.Error switch
        {
            CategoriaOperationError.None => NoContent(),
            CategoriaOperationError.Validation => BadRequest("El identificador no es valido."),
            CategoriaOperationError.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
