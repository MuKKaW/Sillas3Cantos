namespace SillasTresCantos.Api.DTOs;

public class GetProductoArchivoDTO
{
    public required int Id { get; set; }
    public int ProductoId { get; set; }
    public int? SubidoPorUsuarioId { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public DateTime FechaSubida { get; set; }
    public string UrlDescarga { get; set; } = string.Empty;
    public GetProductoArchivoDTO() { }
}
