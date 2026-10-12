using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class SupportService : ISupportService
{
    private readonly INiramoyDbContext _db;

    public SupportService(INiramoyDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<SupportTicketDto>> GetUserTicketsAsync(long userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 50);

        var query = _db.SupportTickets
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Messages)
            .ThenInclude(m => m.Sender)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var dtos = items.Select(MapToSupportTicketDto).ToList();
        return new PagedResult<SupportTicketDto>(dtos, total, page, size);
    }

    public async Task<PagedResult<SupportTicketDto>> GetAllTicketsAsync(string? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);

        var query = _db.SupportTickets
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Messages)
            .ThenInclude(m => m.Sender)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status == status);
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var dtos = items.Select(MapToSupportTicketDto).ToList();
        return new PagedResult<SupportTicketDto>(dtos, total, page, size);
    }

    public async Task<SupportTicketDto> GetTicketByIdAsync(long userId, long ticketId, bool isStaff = false, CancellationToken ct = default)
    {
        var query = _db.SupportTickets
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Messages)
            .ThenInclude(m => m.Sender)
            .Where(t => t.Id == ticketId);

        if (!isStaff)
        {
            query = query.Where(t => t.UserId == userId);
        }

        var ticket = await query.FirstOrDefaultAsync(ct);
        if (ticket == null)
        {
            throw new NotFoundException(nameof(SupportTicket), ticketId);
        }

        return MapToSupportTicketDto(ticket);
    }

    public async Task<SupportTicketDto> CreateTicketAsync(long userId, CreateSupportTicketRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        var ticketNumber = $"TCK-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var ticket = new SupportTicket
        {
            TicketNumber = ticketNumber,
            UserId = userId,
            OrderId = request.OrderId,
            Subject = request.Subject.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "medium" : request.Priority,
            Status = "open",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync(ct);

        var firstMessage = new SupportMessage
        {
            TicketId = ticket.Id,
            SenderId = userId,
            IsStaffReply = false,
            Message = request.InitialMessage.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.SupportMessages.Add(firstMessage);
        await _db.SaveChangesAsync(ct);

        ticket.User = user;
        ticket.Messages.Add(firstMessage);
        firstMessage.Sender = user;

        return MapToSupportTicketDto(ticket);
    }

    public async Task<SupportMessageDto> ReplyTicketAsync(long userId, long ticketId, ReplySupportTicketRequest request, bool isStaff = false, CancellationToken ct = default)
    {
        var ticket = await _db.SupportTickets
            .FirstOrDefaultAsync(t => t.Id == ticketId, ct);

        if (ticket == null || (!isStaff && ticket.UserId != userId))
        {
            throw new NotFoundException(nameof(SupportTicket), ticketId);
        }

        var sender = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        var message = new SupportMessage
        {
            TicketId = ticketId,
            SenderId = userId,
            IsStaffReply = isStaff,
            Message = request.Message.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.SupportMessages.Add(message);

        // Update ticket status
        if (isStaff && ticket.Status == "open")
        {
            ticket.Status = "in_progress";
        }
        else if (!isStaff && ticket.Status == "resolved")
        {
            ticket.Status = "open";
        }
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new SupportMessageDto(
            Id: message.Id,
            TicketId: message.TicketId,
            SenderId: sender.Id,
            SenderName: sender.FullName,
            IsStaffReply: message.IsStaffReply,
            Message: message.Message,
            CreatedAt: message.CreatedAt
        );
    }

    public async Task<SupportTicketDto> UpdateTicketStatusAsync(long staffUserId, long ticketId, UpdateSupportTicketStatusRequest request, CancellationToken ct = default)
    {
        var ticket = await _db.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Messages)
            .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(t => t.Id == ticketId, ct);

        if (ticket == null)
        {
            throw new NotFoundException(nameof(SupportTicket), ticketId);
        }

        ticket.Status = request.Status.Trim().ToLowerInvariant();
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return MapToSupportTicketDto(ticket);
    }

    private static SupportTicketDto MapToSupportTicketDto(SupportTicket t)
    {
        var messageDtos = t.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new SupportMessageDto(
                Id: m.Id,
                TicketId: m.TicketId,
                SenderId: m.SenderId,
                SenderName: m.Sender?.FullName ?? (m.IsStaffReply ? "Support Staff" : "Customer"),
                IsStaffReply: m.IsStaffReply,
                Message: m.Message,
                CreatedAt: m.CreatedAt
            ))
            .ToList();

        return new SupportTicketDto(
            Id: t.Id,
            TicketNumber: t.TicketNumber,
            UserId: t.UserId,
            UserName: t.User?.FullName ?? "Customer",
            UserPhone: t.User?.PhoneNumber ?? "",
            OrderId: t.OrderId,
            OrderNumber: t.Order?.OrderNumber,
            Subject: t.Subject,
            Priority: t.Priority,
            Status: t.Status,
            AssignedTo: t.AssignedTo,
            CreatedAt: t.CreatedAt,
            Messages: messageDtos
        );
    }
}
