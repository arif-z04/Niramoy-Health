using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public interface ISupportService
{
    Task<PagedResult<SupportTicketDto>> GetUserTicketsAsync(long userId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<PagedResult<SupportTicketDto>> GetAllTicketsAsync(string? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<SupportTicketDto> GetTicketByIdAsync(long userId, long ticketId, bool isStaff = false, CancellationToken ct = default);
    Task<SupportTicketDto> CreateTicketAsync(long userId, CreateSupportTicketRequest request, CancellationToken ct = default);
    Task<SupportMessageDto> ReplyTicketAsync(long userId, long ticketId, ReplySupportTicketRequest request, bool isStaff = false, CancellationToken ct = default);
    Task<SupportTicketDto> UpdateTicketStatusAsync(long staffUserId, long ticketId, UpdateSupportTicketStatusRequest request, CancellationToken ct = default);
}
