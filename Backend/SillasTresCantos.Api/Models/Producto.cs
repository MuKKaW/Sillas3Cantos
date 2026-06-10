namespace SillasTresCantos.Api.Models;

public class Producto
{
    public required int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int CategoriaId { get; set; }
    public int MarcaId { get; set; }
    public int? CreadoPorUsuarioId { get; set; }
    public bool EsVisible { get; set; }
    public string? ImagenUrl { get; set; }
    public string? ImagenPublicId { get; set; }
    public string? ImagenResourceType { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public Producto() { }
}
