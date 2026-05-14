namespace SillasTresCantos.Api.DTOs;

public class GetCategoriaDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public GetCategoriaDTO() { }
}

public class PostCategoriaDTO
{
    public required string Nombre { get; set; }
    public PostCategoriaDTO() { }
}

public class PutCategoriaDTO
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public PutCategoriaDTO() { }
}

public class GetCategoriasFiltroDTO
{
    public int IdCategoria { get; set; } = 0;
    public string Nombre { get; set; } = string.Empty;
    public bool OrderAscent { get; set; } = true;
    public GetCategoriasFiltroDTO() { }
}
