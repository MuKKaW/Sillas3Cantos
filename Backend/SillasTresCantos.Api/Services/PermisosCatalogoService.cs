using System.Security.Claims;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class PermisosCatalogoService : IPermisosCatalogoService
{
    private const string UserRole = "User";
    private const string SuperAdminRole = "SuperAdmin";
    private readonly IPermisosCatalogoRepository _permisosRepository;

    public PermisosCatalogoService(IPermisosCatalogoRepository permisosRepository)
    {
        _permisosRepository = permisosRepository;
    }

    public async Task<GetPermisosCatalogoDTO> GetPermisosUserAsync(CancellationToken cancellationToken = default)
    {
        CatalogoPermisos permisos = await _permisosRepository.GetPermisosAsync(UserRole, cancellationToken);
        return MapToGetPermisosCatalogoDTO(permisos);
    }

    public async Task<GetPermisosCatalogoDTO> PutPermisosUserAsync(
        PutPermisosCatalogoDTO permisos,
        CancellationToken cancellationToken = default)
    {
        CatalogoPermisos actualizados = await _permisosRepository.UpdatePermisosAsync(new CatalogoPermisos
        {
            Rol = UserRole,
            ProductosCrear = permisos.Productos.Crear,
            ProductosModificar = permisos.Productos.Modificar,
            ProductosEliminar = permisos.Productos.Eliminar,
            CategoriasCrear = permisos.Categorias.Crear,
            CategoriasModificar = permisos.Categorias.Modificar,
            CategoriasEliminar = permisos.Categorias.Eliminar,
            MarcasCrear = permisos.Marcas.Crear,
            MarcasModificar = permisos.Marcas.Modificar,
            MarcasEliminar = permisos.Marcas.Eliminar,
            SolucionesCrear = permisos.Soluciones?.Crear ?? true,
            SolucionesModificar = permisos.Soluciones?.Modificar ?? true,
            SolucionesEliminar = permisos.Soluciones?.Eliminar ?? true,
            FechaActualizacion = DateTime.UtcNow
        }, cancellationToken);

        return MapToGetPermisosCatalogoDTO(actualizados);
    }

    public async Task<GetPermisosCatalogoDTO> GetPermisosActualesAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        if (IsSuperAdmin(user))
        {
            return MapToGetPermisosCatalogoDTO(SuperAdminPermisos());
        }

        CatalogoPermisos permisos = await _permisosRepository.GetPermisosAsync(UserRole, cancellationToken);
        return MapToGetPermisosCatalogoDTO(permisos);
    }

    public async Task<bool> PuedeGestionarAsync(
        ClaimsPrincipal user,
        CatalogoPermisoEntidad entidad,
        CatalogoPermisoAccion accion,
        CancellationToken cancellationToken = default)
    {
        if (!(user.Identity?.IsAuthenticated ?? false))
        {
            return false;
        }

        if (IsSuperAdmin(user))
        {
            return true;
        }

        CatalogoPermisos permisos = await _permisosRepository.GetPermisosAsync(UserRole, cancellationToken);
        return TienePermiso(permisos, entidad, accion);
    }

    private static bool TienePermiso(CatalogoPermisos permisos, CatalogoPermisoEntidad entidad, CatalogoPermisoAccion accion) =>
        (entidad, accion) switch
        {
            (CatalogoPermisoEntidad.Producto, CatalogoPermisoAccion.Crear) => permisos.ProductosCrear,
            (CatalogoPermisoEntidad.Producto, CatalogoPermisoAccion.Modificar) => permisos.ProductosModificar,
            (CatalogoPermisoEntidad.Producto, CatalogoPermisoAccion.Eliminar) => permisos.ProductosEliminar,
            (CatalogoPermisoEntidad.Categoria, CatalogoPermisoAccion.Crear) => permisos.CategoriasCrear,
            (CatalogoPermisoEntidad.Categoria, CatalogoPermisoAccion.Modificar) => permisos.CategoriasModificar,
            (CatalogoPermisoEntidad.Categoria, CatalogoPermisoAccion.Eliminar) => permisos.CategoriasEliminar,
            (CatalogoPermisoEntidad.Marca, CatalogoPermisoAccion.Crear) => permisos.MarcasCrear,
            (CatalogoPermisoEntidad.Marca, CatalogoPermisoAccion.Modificar) => permisos.MarcasModificar,
            (CatalogoPermisoEntidad.Marca, CatalogoPermisoAccion.Eliminar) => permisos.MarcasEliminar,
            (CatalogoPermisoEntidad.Solucion, CatalogoPermisoAccion.Crear) => permisos.SolucionesCrear,
            (CatalogoPermisoEntidad.Solucion, CatalogoPermisoAccion.Modificar) => permisos.SolucionesModificar,
            (CatalogoPermisoEntidad.Solucion, CatalogoPermisoAccion.Eliminar) => permisos.SolucionesEliminar,
            _ => false
        };

    private static bool IsSuperAdmin(ClaimsPrincipal user) =>
        user.IsInRole(SuperAdminRole)
        || user.Claims.Any(claim => claim.Type == ClaimTypes.Role && claim.Value.Equals(SuperAdminRole, StringComparison.OrdinalIgnoreCase));

    private static CatalogoPermisos SuperAdminPermisos() =>
        new()
        {
            Rol = SuperAdminRole,
            ProductosCrear = true,
            ProductosModificar = true,
            ProductosEliminar = true,
            CategoriasCrear = true,
            CategoriasModificar = true,
            CategoriasEliminar = true,
            MarcasCrear = true,
            MarcasModificar = true,
            MarcasEliminar = true,
            SolucionesCrear = true,
            SolucionesModificar = true,
            SolucionesEliminar = true,
            FechaActualizacion = DateTime.UtcNow
        };

    private static GetPermisosCatalogoDTO MapToGetPermisosCatalogoDTO(CatalogoPermisos permisos) =>
        new()
        {
            Productos = new PermisosTipoCatalogoDTO
            {
                Crear = permisos.ProductosCrear,
                Modificar = permisos.ProductosModificar,
                Eliminar = permisos.ProductosEliminar
            },
            Categorias = new PermisosTipoCatalogoDTO
            {
                Crear = permisos.CategoriasCrear,
                Modificar = permisos.CategoriasModificar,
                Eliminar = permisos.CategoriasEliminar
            },
            Marcas = new PermisosTipoCatalogoDTO
            {
                Crear = permisos.MarcasCrear,
                Modificar = permisos.MarcasModificar,
                Eliminar = permisos.MarcasEliminar
            },
            Soluciones = new PermisosTipoCatalogoDTO
            {
                Crear = permisos.SolucionesCrear,
                Modificar = permisos.SolucionesModificar,
                Eliminar = permisos.SolucionesEliminar
            },
            FechaActualizacion = permisos.FechaActualizacion
        };
}
