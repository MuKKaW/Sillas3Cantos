using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.Models;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Usuario>>> GetUsuarios([FromHeader(Name = "IdUsuario")] int idUsuario = 0, [FromHeader(Name = "Nombre")] string nombre = "", [FromHeader(Name = "OrderAscent")] bool orderAsc = true)
    {
        List<Usuario> usuarios = await _usuarioService.GetUsuariosAsync(idUsuario, nombre, orderAsc);
        return Ok(usuarios);
    }

    [HttpPost]
    public async Task<ActionResult<bool>> PostUsuario([FromBody] Usuario usuario)
    {
        bool creado = await _usuarioService.PostUsuarioAsync(usuario);
        return Ok(creado);
    }

    [HttpPut]
    public async Task<ActionResult<bool>> PutUsuario([FromBody] Usuario usuario)
    {
        bool actualizado = await _usuarioService.PutUsuarioAsync(usuario);
        return Ok(actualizado);
    }

    [HttpDelete]
    public async Task<ActionResult<bool>> DeleteUsuario([FromQuery(Name = "IdUsuario")][Required] int id)
    {
        bool borrado = await _usuarioService.DeleteUsuarioAsync(id);
        return Ok(borrado);
    }
}
