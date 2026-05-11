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
        GetUsuarioDTO? usuario = await _usuarioService.GetUsuarioByIdAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<GetUsuarioDTO>> PostUsuario([FromBody] PostUsuarioDTO usuario)
    {
        UsuarioOperationResult resultado = await _usuarioService.PostUsuarioAsync(usuario);
        if (!resultado.IsSuccess)
        {
            return resultado.Error switch
            {
                UsuarioOperationError.Validation => BadRequest("El email es obligatorio."),
                UsuarioOperationError.Conflict => Conflict("Ya existe un usuario con ese email."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        if (resultado.Usuario is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return CreatedAtAction(nameof(GetUsuarioById), new { id = resultado.Usuario.Id }, resultado.Usuario);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutUsuario(int id, [FromBody] PutUsuarioDTO usuario)
    {
        usuario.Id = id;
        UsuarioOperationResult resultado = await _usuarioService.PutUsuarioAsync(usuario);

        return resultado.Error switch
        {
            UsuarioOperationError.None => NoContent(),
            UsuarioOperationError.Validation => BadRequest("Los datos de usuario no son validos."),
            UsuarioOperationError.NotFound => NotFound(),
            UsuarioOperationError.Conflict => Conflict("Ya existe un usuario con ese email."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        UsuarioOperationResult resultado = await _usuarioService.DeleteUsuarioAsync(id);

        return resultado.Error switch
        {
            UsuarioOperationError.None => NoContent(),
            UsuarioOperationError.Validation => BadRequest("El identificador no es valido."),
            UsuarioOperationError.NotFound => NotFound(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}

