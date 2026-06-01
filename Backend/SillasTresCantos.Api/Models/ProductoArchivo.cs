namespace SillasTresCantos.Api.Models;

public class ProductoArchivo
{
    public required int Id { get; set; }
    public int ProductoId { get; set; }
    public int? SubidoPorUsuarioId { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string NombreAlmacenado { get; set; } = string.Empty;
    public string RutaRelativa { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public DateTime FechaSubida { get; set; }
    public ProductoArchivo() { }
}
