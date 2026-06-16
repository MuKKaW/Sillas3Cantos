using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/permisos/catalogo")]
[Authorize]
public class PermisosCatalogoController : ControllerBase
{
    private readonly IPermisosCatalogoService _permisosCatalogoService;

    public PermisosCatalogoController(IPermisosCatalogoService permisosCatalogoService)
    {
        _permisosCatalogoService = permisosCatalogoService;
    }

    [HttpGet("actuales")]
    public async Task<ActionResult<GetPermisosCatalogoDTO>> GetPermisosActuales(CancellationToken cancellationToken)
    {
        GetPermisosCatalogoDTO permisos = await _permisosCatalogoService.GetPermisosActualesAsync(User, cancellationToken);
        return Ok(permisos);
    }

    [HttpGet("user")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<GetPermisosCatalogoDTO>> GetPermisosUser(CancellationToken cancellationToken)
    {
        GetPermisosCatalogoDTO permisos = await _permisosCatalogoService.GetPermisosUserAsync(cancellationToken);
        return Ok(permisos);
    }

    [HttpPut("user")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<GetPermisosCatalogoDTO>> PutPermisosUser(
        [FromBody] PutPermisosCatalogoDTO permisos,
        CancellationToken cancellationToken)
    {
        GetPermisosCatalogoDTO actualizados = await _permisosCatalogoService.PutPermisosUserAsync(permisos, cancellationToken);
        return Ok(actualizados);
    }
}
