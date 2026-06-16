namespace SillasTresCantos.Api.Models;

public class ConfiguracionCatalogo
{
    public bool UsarFiltroTabs { get; set; }
    public bool MostrarPrecios { get; set; }
    public bool MostrarStock { get; set; }
    public bool MostrarSeccionCatalogo { get; set; }
    public bool MostrarSeccionSoluciones { get; set; }
    public bool MostrarSeccionMapa { get; set; }
    public bool MostrarSeccionConocenos { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public ConfiguracionCatalogo() { }
}
