using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request);
}
