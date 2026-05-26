namespace SillasTresCantos.Api.Models;

public class Marca
{
    public required int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? PaisOrigen { get; set; }
    public int? AnioFundacion { get; set; }
    public bool EsVisible { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public Marca() { }
}
