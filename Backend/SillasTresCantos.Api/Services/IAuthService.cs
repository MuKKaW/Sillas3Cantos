using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface IAuthService
{
    LoginResponseDTO? Login(LoginRequestDTO request);
}
