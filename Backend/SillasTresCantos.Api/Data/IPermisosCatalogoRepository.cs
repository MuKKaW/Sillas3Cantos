using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public interface IPermisosCatalogoRepository
{
    Task<CatalogoPermisos> GetPermisosAsync(string rol, CancellationToken cancellationToken = default);
    Task<CatalogoPermisos> UpdatePermisosAsync(CatalogoPermisos permisos, CancellationToken cancellationToken = default);
}
