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
    public int? CreadoPorUsuarioId { get; set; }
    public bool EsVisible { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
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
    public bool? EsVisible { get; set; }
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
    public bool? EsVisible { get; set; }
    public PutProductoDTO() { }
}

public class GetProductosFiltroDTO
{
    public int IdProducto { get; set; } = 0;
    public string Nombre { get; set; } = string.Empty;
    public int CategoriaId { get; set; } = 0;
    public int MarcaId { get; set; } = 0;
    public int CreadoPorUsuarioId { get; set; } = 0;
    public bool OrderAscent { get; set; } = true;
    public bool IncludeHidden { get; set; } = false;
    public GetProductosFiltroDTO() { }
}
