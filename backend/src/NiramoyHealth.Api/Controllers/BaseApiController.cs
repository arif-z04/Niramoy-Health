using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Domain.Enums;

namespace NiramoyHealth.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected long? CurrentUserId
    {
        get
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(idClaim, out var id) ? id : null;
        }
    }

    protected long RequiredUserId =>
        CurrentUserId ?? throw new NiramoyHealth.Domain.Exceptions.UnauthorizedException("User is not authenticated.");

    protected bool IsStaffUser =>
        User.IsInRole(RoleNames.Admin) ||
        User.IsInRole(RoleNames.SuperAdmin) ||
        User.IsInRole(RoleNames.Pharmacist) ||
        User.IsInRole(RoleNames.SupportAgent);

    protected string? ClientIpAddress =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    protected string? ClientUserAgent =>
        Request.Headers.UserAgent.ToString();

    protected Guid? GuestTokenHeader
    {
        get
        {
            if (Request.Headers.TryGetValue("X-Guest-Token", out var tokenStr) &&
                Guid.TryParse(tokenStr, out var token))
            {
                return token;
            }
            return null;
        }
    }

    protected IActionResult OkResponse<T>(T data, string message = "Success") =>
        Ok(ApiResponse<T>.Ok(data, message));
}
