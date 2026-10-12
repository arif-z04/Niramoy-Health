using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;

namespace NiramoyHealth.Api.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var response = await _authService.RegisterAsync(request, ClientIpAddress, ClientUserAgent, ct);
        return OkResponse(response, "Registration successful.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ClientIpAddress, ClientUserAgent, ct);
        return OkResponse(response, "Login successful.");
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken, ClientIpAddress, ClientUserAgent, ct);
        return OkResponse(response, "Token refreshed successfully.");
    }

    [Authorize]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken, ct);
        return OkResponse(true, "Token revoked successfully.");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var user = await _authService.GetCurrentUserAsync(RequiredUserId, ct);
        return OkResponse(user);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var profile = await _authService.GetUserProfileAsync(RequiredUserId, ct);
        return OkResponse(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var profile = await _authService.UpdateUserProfileAsync(RequiredUserId, request, ct);
        return OkResponse(profile, "Profile updated successfully.");
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(RequiredUserId, request, ct);
        return OkResponse(true, "Password changed successfully.");
    }
}
