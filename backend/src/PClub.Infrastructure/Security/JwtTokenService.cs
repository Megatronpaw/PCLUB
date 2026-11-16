using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PClub.Application.Abstractions;
using PClub.Domain.Entities;

namespace PClub.Infrastructure.Security;

/// <summary>Выпуск токенов доступа и обновления.</summary>
public sealed class JwtTokenService : ITokenService
{
    /// <summary>Длина значения refresh-токена в байтах.</summary>
    private const int RefreshTokenSize = 32;

    private readonly JwtOptions _options;
    private readonly IClock _clock;

    /// <summary>Создаёт сервис.</summary>
    public JwtTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    /// <inheritdoc />
    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(User user)
    {
        var now = _clock.UtcNow;
        var expiresAt = now + _options.AccessTokenLifetime;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("name", user.DisplayName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    /// <inheritdoc />
    public (string Value, string Hash) CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(RefreshTokenSize);
        var value = Base64UrlEncoder.Encode(bytes);

        return (value, HashRefreshToken(value));
    }

    /// <inheritdoc />
    public string HashRefreshToken(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));

        return Convert.ToBase64String(hash);
    }
}
