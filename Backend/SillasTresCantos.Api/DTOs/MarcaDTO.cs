namespace SillasTresCantos.Api.DTOs;

public class GetMarcaDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? PaisOrigen { get; set; }
    public int? AnioFundacion { get; set; }
    public int OrdenVisual { get; set; }
    public bool EsVisible { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public GetMarcaDTO() { }
}

public class PostMarcaDTO
{
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? PaisOrigen { get; set; }
    public int? AnioFundacion { get; set; }
    public int? OrdenVisual { get; set; }
    public bool? EsVisible { get; set; }
    public PostMarcaDTO() { }
}

public class PutMarcaDTO
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? PaisOrigen { get; set; }
    public int? AnioFundacion { get; set; }
    public int? OrdenVisual { get; set; }
    public bool? EsVisible { get; set; }
    public PutMarcaDTO() { }
}

public class GetMarcasFiltroDTO
{
    public int IdMarca { get; set; } = 0;
    public string Nombre { get; set; } = "";
    public bool OrderAscent { get; set; } = true;
    public bool IncludeHidden { get; set; } = false;
    public GetMarcasFiltroDTO() { }
}
