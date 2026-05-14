using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using SillasTresCantos.Api.Configuration;
using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public class AuthService : IAuthService
{
    private readonly JwtOptions _jwtOptions;
    private readonly AuthOptions _authOptions;

    public AuthService(IOptions<JwtOptions> jwtOptions, IOptions<AuthOptions> authOptions)
    {
        _jwtOptions = jwtOptions.Value;
        _authOptions = authOptions.Value;
    }

    public LoginResponseDTO? Login(LoginRequestDTO request)
    {
        string username = request.Username?.Trim() ?? string.Empty;
        string password = request.Password ?? string.Empty;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        AuthUserOptions? user = _authOptions.Users.FirstOrDefault(x =>
            string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Password, password, StringComparison.Ordinal));

        if (user is null)
        {
            return null;
        }

        byte[] keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.Key);
        DateTime expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
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
            Role = user.Role
        };
    }
}
