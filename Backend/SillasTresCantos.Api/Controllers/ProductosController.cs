using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetProductoDTO>>> GetProductos(
        [FromQuery] int idProducto = 0,
        [FromQuery] string nombre = "",
        [FromQuery] int categoriaId = 0,
        [FromQuery] int marcaId = 0,
        [FromQuery] bool orderAsc = true,
        [FromQuery] bool includeHidden = false)
    {
        if (includeHidden && !(User.Identity?.IsAuthenticated ?? false))
        {
            return Unauthorized("Debes autenticarte para consultar productos ocultos.");
        }

        GetProductosFiltroDTO filtro = new()
        {
            IdProducto = idProducto,
            Nombre = nombre,
            CategoriaId = categoriaId,
            MarcaId = marcaId,
            OrderAscent = orderAsc,
            IncludeHidden = includeHidden
        };

        List<GetProductoDTO> productos = await _productoService.GetProductosAsync(filtro);
        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetProductoDTO>> GetProductoById(int id, [FromQuery] bool includeHidden = false)
    {
        if (includeHidden && !(User.Identity?.IsAuthenticated ?? false))
        {
            return Unauthorized("Debes autenticarte para consultar productos ocultos.");
        }

        GetProductoDTO? producto = await _productoService.GetProductoByIdAsync(id);
        if (producto is null)
        {
            return NotFound();
        }

        if (!includeHidden && !producto.EsVisible)
        {
            return NotFound();
        }

        return Ok(producto);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<GetProductoDTO>> PostProducto([FromBody] PostProductoDTO producto)
    {
        ProductoOperationResult resultado = await _productoService.PostProductoAsync(producto);
        if (!resultado.IsSuccess)
        {
            return resultado.Error switch
            {
                ProductoOperationError.Validation => BadRequest("Los datos del producto no son validos."),
                ProductoOperationError.RelatedNotFound => BadRequest("La categoria o la marca indicada no existe."),
                ProductoOperationError.Conflict => Conflict("Existe un conflicto con los datos del producto."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        if (resultado.Producto is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return CreatedAtAction(nameof(GetProductoById), new { id = resultado.Producto.Id }, resultado.Producto);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> PutProducto(int id, [FromBody] PutProductoDTO producto)
    {
        producto.Id = id;
        ProductoOperationResult resultado = await _productoService.PutProductoAsync(producto);

        return resultado.Error switch
        {
            ProductoOperationError.None => NoContent(),
            ProductoOperationError.Validation => BadRequest("Los datos del producto no son validos."),
            ProductoOperationError.NotFound => NotFound(),
            ProductoOperationError.RelatedNotFound => BadRequest("La categoria o la marca indicada no existe."),
            ProductoOperationError.Conflict => Conflict("Existe un conflicto con los datos del producto."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        ProductoOperationResult resultado = await _productoService.DeleteProductoAsync(id);

        return resultado.Error switch
        {
            ProductoOperationError.None => NoContent(),
            ProductoOperationError.Validation => BadRequest("El identificador no es valido."),
            ProductoOperationError.NotFound => NotFound(),
            ProductoOperationError.Conflict => Conflict("No se puede eliminar el producto porque tiene datos relacionados."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
