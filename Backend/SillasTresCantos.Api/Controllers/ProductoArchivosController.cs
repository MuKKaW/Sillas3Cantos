using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/productos/{productoId:int}/archivos")]
public class ProductoArchivosController : ControllerBase
{
    private readonly IProductoArchivoService _productoArchivoService;

    public ProductoArchivosController(IProductoArchivoService productoArchivoService)
    {
        _productoArchivoService = productoArchivoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetProductoArchivoDTO>>> GetArchivosProducto(
        int productoId,
        [FromQuery] bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        if (includeHidden && !(User.Identity?.IsAuthenticated ?? false))
        {
            return Unauthorized("Debes autenticarte para consultar archivos de productos ocultos.");
        }

        ProductoArchivoOperationResult resultado =
            await _productoArchivoService.GetArchivosAsync(productoId, includeHidden, cancellationToken);

        return resultado.Error switch
        {
            ProductoArchivoOperationError.None when resultado.Archivos is not null => Ok(resultado.Archivos),
            ProductoArchivoOperationError.Validation => BadRequest("El identificador de producto no es valido."),
            ProductoArchivoOperationError.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("{archivoId:int}")]
    public async Task<IActionResult> DownloadArchivo(
        int productoId,
        int archivoId,
        [FromQuery] bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        if (includeHidden && !(User.Identity?.IsAuthenticated ?? false))
        {
            return Unauthorized("Debes autenticarte para consultar archivos de productos ocultos.");
        }

        ProductoArchivoOperationResult resultado =
            await _productoArchivoService.GetArchivoDescargaAsync(productoId, archivoId, includeHidden, cancellationToken);

        return resultado.Error switch
        {
            ProductoArchivoOperationError.None
                when !string.IsNullOrWhiteSpace(resultado.AbsoluteFilePath)
                     && !string.IsNullOrWhiteSpace(resultado.DownloadContentType)
                     && !string.IsNullOrWhiteSpace(resultado.DownloadFileName)
                => PhysicalFile(resultado.AbsoluteFilePath, resultado.DownloadContentType, resultado.DownloadFileName),
            ProductoArchivoOperationError.Validation => BadRequest("El identificador no es valido."),
            ProductoArchivoOperationError.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<GetProductoArchivoDTO>> UploadArchivo(
        int productoId,
        IFormFile archivo,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUsuarioAutenticadoId(out int usuarioId))
        {
            return Unauthorized("No se pudo identificar al usuario autenticado. Vuelve a iniciar sesion.");
        }

        ProductoArchivoOperationResult resultado =
            await _productoArchivoService.UploadArchivoAsync(productoId, archivo, usuarioId, cancellationToken);

        if (!resultado.IsSuccess)
        {
            return resultado.Error switch
            {
                ProductoArchivoOperationError.Validation => BadRequest("No se ha recibido un archivo valido."),
                ProductoArchivoOperationError.NotFound => NotFound("El producto indicado no existe."),
                ProductoArchivoOperationError.UnsupportedType => BadRequest("El tipo de archivo no esta permitido."),
                ProductoArchivoOperationError.FileTooLarge => StatusCode(StatusCodes.Status413PayloadTooLarge, "El archivo supera el tamano maximo permitido."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        if (resultado.Archivo is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return CreatedAtAction(
            nameof(DownloadArchivo),
            new { productoId = resultado.Archivo.ProductoId, archivoId = resultado.Archivo.Id },
            resultado.Archivo);
    }

    [HttpDelete("{archivoId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteArchivo(int productoId, int archivoId, CancellationToken cancellationToken = default)
    {
        ProductoArchivoOperationResult resultado =
            await _productoArchivoService.DeleteArchivoAsync(productoId, archivoId, cancellationToken);

        return resultado.Error switch
        {
            ProductoArchivoOperationError.None => NoContent(),
            ProductoArchivoOperationError.Validation => BadRequest("El identificador no es valido."),
            ProductoArchivoOperationError.NotFound => NotFound(),
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
