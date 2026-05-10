namespace SillasTresCantos.Api.DTOs;

public class GetUsuarioDTO
{
    public required int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public required string Email { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public bool? EstaActivo { get; set; }
    public GetUsuarioDTO() { }
}

public class PostUsuarioDTO
{
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public required string Email { get; set; }
    public PostUsuarioDTO() { }
}

public class PutUsuarioDTO
{
    public required int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Email { get; set; }
    public bool? EstaActivo { get; set; }
    public PutUsuarioDTO() { }
}

public class GetUsuariosFiltroDTO
{
    public int IdUsuario { get; set; } = 0;
    public string Nombre { get; set; } = "";
    public bool OrderAscent { get; set; } = true;
    public GetUsuariosFiltroDTO() { }
}
