namespace SillasTresCantos.Api.Models;

public class Solucion
{
    public required int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public int OrdenVisual { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public Solucion() { }
}
