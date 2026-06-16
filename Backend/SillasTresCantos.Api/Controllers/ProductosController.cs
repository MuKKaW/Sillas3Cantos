using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly IPermisosCatalogoService _permisosCatalogoService;

    public ProductosController(IProductoService productoService, IPermisosCatalogoService permisosCatalogoService)
    {
        _productoService = productoService;
        _permisosCatalogoService = permisosCatalogoService;
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

    [HttpGet("mios")]
    [Authorize]
    public async Task<ActionResult<List<GetProductoDTO>>> GetMisProductos(
        [FromQuery] bool orderAsc = true,
        [FromQuery] bool includeHidden = true)
    {
        if (!TryGetUsuarioAutenticadoId(out int usuarioId))
        {
            return Unauthorized("No se pudo identificar al usuario autenticado. Vuelve a iniciar sesion.");
        }

        GetProductosFiltroDTO filtro = new()
        {
            CreadoPorUsuarioId = usuarioId,
            OrderAscent = orderAsc,
            IncludeHidden = includeHidden
        };

        List<GetProductoDTO> productos = await _productoService.GetProductosAsync(filtro);
        return Ok(productos);
    }

    [HttpGet("usuario/{usuarioId:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<GetProductoDTO>>> GetProductosPorUsuario(
        int usuarioId,
        [FromQuery] bool orderAsc = true,
        [FromQuery] bool includeHidden = true)
    {
        if (usuarioId <= 0)
        {
            return BadRequest("El identificador de usuario no es valido.");
        }

        GetProductosFiltroDTO filtro = new()
        {
            CreadoPorUsuarioId = usuarioId,
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
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<GetProductoDTO>> PostProducto(
        [FromForm] PostProductoDTO producto,
        CancellationToken cancellationToken = default)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Producto, CatalogoPermisoAccion.Crear, cancellationToken))
        {
            return Forbid();
        }

        if (!TryGetUsuarioAutenticadoId(out int usuarioId))
        {
            return Unauthorized("No se pudo identificar al usuario autenticado. Vuelve a iniciar sesion.");
        }

        ProductoOperationResult resultado = await _productoService.PostProductoAsync(producto, usuarioId, cancellationToken);
        if (!resultado.IsSuccess)
        {
            return resultado.Error switch
            {
                ProductoOperationError.Validation => BadRequest("Los datos del producto o la imagen no son validos."),
                ProductoOperationError.RelatedNotFound => BadRequest("La categoria o la marca indicada no existe."),
                ProductoOperationError.Conflict => Conflict("Existe un conflicto con los datos del producto."),
                ProductoOperationError.UnsupportedType => BadRequest("La imagen debe ser JPG, PNG o WEBP."),
                ProductoOperationError.FileTooLarge => StatusCode(StatusCodes.Status413PayloadTooLarge, "La imagen supera el tamano maximo permitido."),
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
    [Consumes("application/json")]
    public async Task<IActionResult> PutProducto(int id, [FromBody] PutProductoDTO producto)
    {
        return await PutProductoCoreAsync(id, producto, HttpContext.RequestAborted);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PutProductoConImagen(
        int id,
        [FromForm] PutProductoDTO producto,
        CancellationToken cancellationToken = default)
    {
        return await PutProductoCoreAsync(id, producto, cancellationToken);
    }

    private async Task<IActionResult> PutProductoCoreAsync(
        int id,
        PutProductoDTO producto,
        CancellationToken cancellationToken)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Producto, CatalogoPermisoAccion.Modificar, cancellationToken))
        {
            return Forbid();
        }

        producto.Id = id;
        ProductoOperationResult resultado = await _productoService.PutProductoAsync(producto, cancellationToken);

        return resultado.Error switch
        {
            ProductoOperationError.None => NoContent(),
            ProductoOperationError.Validation => BadRequest("Los datos del producto no son validos."),
            ProductoOperationError.NotFound => NotFound(),
            ProductoOperationError.RelatedNotFound => BadRequest("La categoria o la marca indicada no existe."),
            ProductoOperationError.Conflict => Conflict("Existe un conflicto con los datos del producto."),
            ProductoOperationError.UnsupportedType => BadRequest("La imagen debe ser JPG, PNG o WEBP."),
            ProductoOperationError.FileTooLarge => StatusCode(StatusCodes.Status413PayloadTooLarge, "La imagen supera el tamano maximo permitido."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        if (!await _permisosCatalogoService.PuedeGestionarAsync(User, CatalogoPermisoEntidad.Producto, CatalogoPermisoAccion.Eliminar, HttpContext.RequestAborted))
        {
            return Forbid();
        }

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

    private bool TryGetUsuarioAutenticadoId(out int usuarioId)
    {
        List<string> claimValues = User.Claims
            .Where(claim => claim.Type == ClaimTypes.NameIdentifier || claim.Type == "nameid")
            .Select(claim => claim.Value)
            .ToList();

        foreach (string value in claimValues)
        {
            if (int.TryParse(value, out usuarioId) && usuarioId > 0)
            {
                return true;
            }
        }

        usuarioId = 0;
        return false;
    }
}
