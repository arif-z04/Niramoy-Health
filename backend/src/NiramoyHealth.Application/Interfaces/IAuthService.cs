using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<UserDto> GetCurrentUserAsync(long userId, CancellationToken ct = default);
    Task<UserProfileDto> GetUserProfileAsync(long userId, CancellationToken ct = default);
    Task<UserProfileDto> UpdateUserProfileAsync(long userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken ct = default);
}
