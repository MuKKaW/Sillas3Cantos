namespace SillasTresCantos.Api.DTOs;

public class PermisosTipoCatalogoDTO
{
    public bool Crear { get; set; }
    public bool Modificar { get; set; }
    public bool Eliminar { get; set; }
    public PermisosTipoCatalogoDTO() { }
}

public class GetPermisosCatalogoDTO
{
    public required PermisosTipoCatalogoDTO Productos { get; set; }
    public required PermisosTipoCatalogoDTO Categorias { get; set; }
    public required PermisosTipoCatalogoDTO Marcas { get; set; }
    public required PermisosTipoCatalogoDTO Soluciones { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public GetPermisosCatalogoDTO() { }
}

public class PutPermisosCatalogoDTO
{
    public required PermisosTipoCatalogoDTO Productos { get; set; }
    public required PermisosTipoCatalogoDTO Categorias { get; set; }
    public required PermisosTipoCatalogoDTO Marcas { get; set; }
    public required PermisosTipoCatalogoDTO Soluciones { get; set; }
    public PutPermisosCatalogoDTO() { }
}
