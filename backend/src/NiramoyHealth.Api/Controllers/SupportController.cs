using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Enums;

namespace NiramoyHealth.Api.Controllers;

[Authorize]
public class SupportController : BaseApiController
{
    private readonly ISupportService _supportService;

    public SupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    [HttpGet("tickets")]
    public async Task<IActionResult> GetTickets(
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (IsStaffUser)
        {
            var result = await _supportService.GetAllTicketsAsync(status, pageNumber, pageSize, ct);
            return Ok(result);
        }
        else
        {
            var result = await _supportService.GetUserTicketsAsync(RequiredUserId, pageNumber, pageSize, ct);
            return Ok(result);
        }
    }

    [HttpGet("tickets/{id:long}")]
    public async Task<IActionResult> GetTicketById(long id, CancellationToken ct)
    {
        var ticket = await _supportService.GetTicketByIdAsync(RequiredUserId, id, IsStaffUser, ct);
        return OkResponse(ticket);
    }

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketRequest request, CancellationToken ct)
    {
        var ticket = await _supportService.CreateTicketAsync(RequiredUserId, request, ct);
        return OkResponse(ticket, "Support ticket created successfully.");
    }

    [HttpPost("tickets/{id:long}/reply")]
    public async Task<IActionResult> ReplyTicket(long id, [FromBody] ReplySupportTicketRequest request, CancellationToken ct)
    {
        var message = await _supportService.ReplyTicketAsync(RequiredUserId, id, request, IsStaffUser, ct);
        return OkResponse(message, "Reply posted successfully.");
    }

    [Authorize(Roles = $"{RoleNames.SupportAgent},{RoleNames.Admin},{RoleNames.SuperAdmin}")]
    [HttpPut("tickets/{id:long}/status")]
    public async Task<IActionResult> UpdateTicketStatus(long id, [FromBody] UpdateSupportTicketStatusRequest request, CancellationToken ct)
    {
        var ticket = await _supportService.UpdateTicketStatusAsync(RequiredUserId, id, request, ct);
        return OkResponse(ticket, $"Ticket status updated to {ticket.Status}.");
    }
}
