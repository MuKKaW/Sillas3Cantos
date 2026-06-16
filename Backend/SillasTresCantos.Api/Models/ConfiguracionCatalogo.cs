namespace SillasTresCantos.Api.Models;

public class ConfiguracionCatalogo
{
    public bool UsarFiltroTabs { get; set; }
    public bool MostrarPrecios { get; set; }
    public bool MostrarStock { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public ConfiguracionCatalogo() { }
}
