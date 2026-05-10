namespace SillasTresCantos.Api.Models;

public class Usuario
{
    public required int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public bool? EstaActivo { get; set; }
    public Usuario() { }
}
