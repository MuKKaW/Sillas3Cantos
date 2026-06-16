using System.Security.Claims;
using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public enum CatalogoPermisoEntidad
{
    Producto,
    Categoria,
    Marca,
    Solucion
}

public enum CatalogoPermisoAccion
{
    Crear,
    Modificar,
    Eliminar
}

public interface IPermisosCatalogoService
{
    Task<GetPermisosCatalogoDTO> GetPermisosUserAsync(CancellationToken cancellationToken = default);
    Task<GetPermisosCatalogoDTO> PutPermisosUserAsync(PutPermisosCatalogoDTO permisos, CancellationToken cancellationToken = default);
    Task<GetPermisosCatalogoDTO> GetPermisosActualesAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
    Task<bool> PuedeGestionarAsync(
        ClaimsPrincipal user,
        CatalogoPermisoEntidad entidad,
        CatalogoPermisoAccion accion,
        CancellationToken cancellationToken = default);
}
