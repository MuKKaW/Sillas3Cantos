namespace SillasTresCantos.Api.DTOs;

public class GetConfiguracionCatalogoDTO
{
    public bool UsarFiltroTabs { get; set; }
    public bool MostrarPrecios { get; set; }
    public bool MostrarStock { get; set; }
    public bool MostrarSeccionCatalogo { get; set; }
    public bool MostrarSeccionSoluciones { get; set; }
    public bool MostrarSeccionMapa { get; set; }
    public bool MostrarSeccionConocenos { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public GetConfiguracionCatalogoDTO() { }
}

public class PutConfiguracionCatalogoDTO
{
    public bool UsarFiltroTabs { get; set; }
    public bool MostrarPrecios { get; set; }
    public bool MostrarStock { get; set; }
    public bool MostrarSeccionCatalogo { get; set; }
    public bool MostrarSeccionSoluciones { get; set; }
    public bool MostrarSeccionMapa { get; set; }
    public bool MostrarSeccionConocenos { get; set; }
    public PutConfiguracionCatalogoDTO() { }
}
