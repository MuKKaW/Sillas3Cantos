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
    public Producto() { }
}
