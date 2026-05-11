using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetUsuarioDTO>>> GetUsuarios([FromQuery] string nombre = "", [FromQuery] bool orderAsc = true)
    {
        GetUsuariosFiltroDTO filtro = new()
        {
            Nombre = nombre,
            OrderAscent = orderAsc
        };
        List<GetUsuarioDTO> usuarios = await _usuarioService.GetUsuariosAsync(filtro);
        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetUsuarioDTO>> GetUsuarioById(int id)
    {
        GetUsuariosFiltroDTO filtro = new()
        {
            IdUsuario = id
        };

        GetUsuarioDTO? usuario = (await _usuarioService.GetUsuariosAsync(filtro)).FirstOrDefault();
        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<bool>> PostUsuario([FromBody] PostUsuarioDTO usuario)
    {
        bool creado = await _usuarioService.PostUsuarioAsync(usuario);
        return Ok(creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> PutUsuario(int id, [FromBody] PutUsuarioDTO usuario)
    {
        usuario.Id = id;
        bool actualizado = await _usuarioService.PutUsuarioAsync(usuario);
        return Ok(actualizado);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<bool>> DeleteUsuario(int id)
    {
        bool borrado = await _usuarioService.DeleteUsuarioAsync(id);
        return Ok(borrado);
    }
}
