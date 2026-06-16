namespace SillasTresCantos.Api.Models;

public class CatalogoPermisos
{
    public required string Rol { get; set; }
    public bool ProductosCrear { get; set; }
    public bool ProductosModificar { get; set; }
    public bool ProductosEliminar { get; set; }
    public bool CategoriasCrear { get; set; }
    public bool CategoriasModificar { get; set; }
    public bool CategoriasEliminar { get; set; }
    public bool MarcasCrear { get; set; }
    public bool MarcasModificar { get; set; }
    public bool MarcasEliminar { get; set; }
    public bool SolucionesCrear { get; set; }
    public bool SolucionesModificar { get; set; }
    public bool SolucionesEliminar { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public CatalogoPermisos() { }
}
