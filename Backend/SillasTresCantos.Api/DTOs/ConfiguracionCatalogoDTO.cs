namespace SillasTresCantos.Api.DTOs;

public class GetConfiguracionCatalogoDTO
{
    public bool UsarFiltroTabs { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public GetConfiguracionCatalogoDTO() { }
}

public class PutConfiguracionCatalogoDTO
{
    public bool UsarFiltroTabs { get; set; }
    public PutConfiguracionCatalogoDTO() { }
}
