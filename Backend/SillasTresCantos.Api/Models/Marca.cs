namespace SillasTresCantos.Api.Models;

public class Marca
{
    public required int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Marca() { }
}
