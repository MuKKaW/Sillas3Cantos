using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using SillasTresCantos.Api.Configuration;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class AuthService : IAuthService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IOptions<JwtOptions> jwtOptions, IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher)
    {
        _jwtOptions = jwtOptions.Value;
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO request)
    {
        string username = request.Username?.Trim() ?? string.Empty;
        string password = request.Password ?? string.Empty;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        Usuario? user = await _usuarioRepository.GetUsuarioByUsernameAsync(username);
        if (user is null || user.EstaActivo != true)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        bool isValidPassword = _passwordHasher.Verify(password, user.PasswordHash);
        if (!isValidPassword)
        {
            return null;
        }

        string role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role;
        string claimUserName = user.Username ?? username;

        byte[] keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.Key);
        DateTime expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, claimUserName),
            new(JwtRegisteredClaimNames.UniqueName, claimUserName),
            new(ClaimTypes.Name, claimUserName),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        SigningCredentials credentials = new(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        return new LoginResponseDTO
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expires,
            Role = role
        };
    }
}
