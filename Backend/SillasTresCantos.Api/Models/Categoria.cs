namespace SillasTresCantos.Api.Models;

public class Categoria
{
    public required int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Categoria() { }
}
