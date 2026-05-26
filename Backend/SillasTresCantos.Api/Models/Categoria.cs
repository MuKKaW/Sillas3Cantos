namespace SillasTresCantos.Api.Models;

public class Categoria
{
    public required int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int OrdenVisual { get; set; }
    public bool EsVisible { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public Categoria() { }
}
