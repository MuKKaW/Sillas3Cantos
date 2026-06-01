namespace SillasTresCantos.Api.DTOs;

public class GetProductoExternoDTO
{
    public int IdExterno { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public required string Categoria { get; set; }
    public string? Marca { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public double Rating { get; set; }
    public string? Thumbnail { get; set; }
    public GetProductoExternoDTO() { }
}

public class GetBusquedaProductosExternosDTO
{
    public required string Fuente { get; set; }
    public required string Query { get; set; }
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Limit { get; set; }
    public required List<GetProductoExternoDTO> Productos { get; set; }
    public GetBusquedaProductosExternosDTO() { }
}

public class BuscarProductosExternosFiltroDTO
{
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
    public int Skip { get; set; } = 0;
    public BuscarProductosExternosFiltroDTO() { }
}
