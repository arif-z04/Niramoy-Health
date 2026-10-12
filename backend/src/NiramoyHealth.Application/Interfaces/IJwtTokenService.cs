using NiramoyHealth.Domain.Entities;

namespace NiramoyHealth.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, int ExpiresIn) GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
