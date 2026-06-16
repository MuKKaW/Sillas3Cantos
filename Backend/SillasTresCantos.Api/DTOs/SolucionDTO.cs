namespace SillasTresCantos.Api.DTOs;

public class GetSolucionDTO
{
    public required int Id { get; set; }
    public required string Titulo { get; set; }
    public required string Texto { get; set; }
    public required string Emoji { get; set; }
    public int OrdenVisual { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public GetSolucionDTO() { }
}

public class PostSolucionDTO
{
    public required string Titulo { get; set; }
    public required string Texto { get; set; }
    public required string Emoji { get; set; }
    public int? OrdenVisual { get; set; }
    public PostSolucionDTO() { }
}

public class PutSolucionDTO
{
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public string? Texto { get; set; }
    public string? Emoji { get; set; }
    public int? OrdenVisual { get; set; }
    public PutSolucionDTO() { }
}

public class GetSolucionesFiltroDTO
{
    public int IdSolucion { get; set; } = 0;
    public string Titulo { get; set; } = string.Empty;
    public bool OrderAscent { get; set; } = true;
    public GetSolucionesFiltroDTO() { }
}
