namespace SillasTresCantos.Api.DTOs;

public class GetProductoDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int CategoriaId { get; set; }
    public int MarcaId { get; set; }
    public GetProductoDTO() { }
}

public class PostProductoDTO
{
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int CategoriaId { get; set; }
    public int MarcaId { get; set; }
    public PostProductoDTO() { }
}

public class PutProductoDTO
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal? Precio { get; set; }
    public int? Stock { get; set; }
    public int? CategoriaId { get; set; }
    public int? MarcaId { get; set; }
    public PutProductoDTO() { }
}

public class GetProductosFiltroDTO
{
    public int IdProducto { get; set; } = 0;
    public string Nombre { get; set; } = string.Empty;
    public bool OrderAscent { get; set; } = true;
    public GetProductosFiltroDTO() { }
}
