using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Entities;

namespace NiramoyHealth.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, int ExpiresIn) GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var secret = _config["Jwt:SecretKey"] ?? "NiramoyHealth_SuperSecret_Jwt_EncryptionKey_2026_Bangladesh_Security!";
        var issuer = _config["Jwt:Issuer"] ?? "NiramoyHealth";
        var audience = _config["Jwt:Audience"] ?? "NiramoyHealthStorefront";
        var expirationMinutes = int.TryParse(_config["Jwt:AccessTokenExpirationMinutes"], out var mins) ? mins : 120;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.MobilePhone, user.PhoneNumber)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new(ClaimTypes.Email, user.Email));
        }

        foreach (var role in roles)
        {
            claims.Add(new(ClaimTypes.Role, role));
        }

        var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expirationMinutes * 60);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
