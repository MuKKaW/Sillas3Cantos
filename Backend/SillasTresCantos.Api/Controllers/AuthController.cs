using Microsoft.AspNetCore.Mvc;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Services;

namespace SillasTresCantos.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponseDTO> Login([FromBody] LoginRequestDTO request)
    {
        LoginResponseDTO? response = _authService.Login(request);
        if (response is null)
        {
            return Unauthorized("Credenciales invalidas.");
        }

        return Ok(response);
    }
}
