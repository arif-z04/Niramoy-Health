using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Enums;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly INiramoyDbContext _db;
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentService(INiramoyDbContext db, IEnumerable<IPaymentGateway> gateways)
    {
        _db = db;
        _gateways = gateways;
    }

    public async Task<PaymentInitResultDto> InitiatePaymentAsync(long userId, long orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.PaymentMethod)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);

        if (order == null)
        {
            throw new NotFoundException(nameof(Order), orderId);
        }

        if (order.PaymentStatus == PaymentStatuses.Paid)
        {
            throw new BusinessRuleViolationException("This order is already paid.");
        }

        var payment = order.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        if (payment == null)
        {
            payment = new Payment
            {
                PublicId = Guid.NewGuid(),
                OrderId = order.Id,
                PaymentMethodId = order.PaymentMethodId,
                PaymentStatus = PaymentTransactionStatuses.Pending,
                Amount = order.TotalAmount,
                Currency = order.Currency,
                GatewayProvider = order.PaymentMethod?.Code ?? "cod",
                InitiatedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(ct);
        }

        var providerCode = (order.PaymentMethod?.Code ?? "cod").ToLowerInvariant();
        var gateway = _gateways.FirstOrDefault(g => g.ProviderCode.Equals(providerCode, StringComparison.OrdinalIgnoreCase))
            ?? _gateways.FirstOrDefault(g => g.ProviderCode.Equals("cod", StringComparison.OrdinalIgnoreCase))
            ?? throw new BusinessRuleViolationException($"Payment gateway '{providerCode}' is not supported.");

        var initResult = await gateway.InitializePaymentAsync(payment, order, ct);

        payment.GatewayResponseJson = initResult.RawResponseJson;
        payment.GatewayProvider = gateway.ProviderCode;

        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            PaymentId = payment.Id,
            TransactionType = "initiate",
            Status = initResult.Success ? "success" : "failed",
            ProviderReference = initResult.GatewayPaymentId,
            RawResponseSanitizedJson = initResult.RawResponseJson,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return new PaymentInitResultDto(
            PaymentPublicId: payment.PublicId,
            OrderId: order.Id,
            OrderNumber: order.OrderNumber,
            GatewayProvider: gateway.ProviderCode,
            Amount: payment.Amount,
            Currency: payment.Currency,
            GatewayPaymentId: initResult.GatewayPaymentId,
            RedirectUrl: initResult.RedirectUrl,
            Status: payment.PaymentStatus
        );
    }

    public async Task<PaymentResultDto> ExecutePaymentAsync(ExecutePaymentRequest request, CancellationToken ct = default)
    {
        var payment = await _db.Payments
            .Include(p => p.Order)
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.PublicId == request.PaymentPublicId, ct);

        if (payment == null)
        {
            throw new NotFoundException(nameof(Payment), request.PaymentPublicId);
        }

        var providerCode = (payment.PaymentMethod?.Code ?? payment.GatewayProvider ?? "cod").ToLowerInvariant();
        var gateway = _gateways.FirstOrDefault(g => g.ProviderCode.Equals(providerCode, StringComparison.OrdinalIgnoreCase))
            ?? throw new BusinessRuleViolationException($"Payment provider '{providerCode}' is unavailable.");

        var execResult = await gateway.ExecutePaymentAsync(payment, request.GatewayPaymentId, request.ProviderTrxId, ct);

        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            PaymentId = payment.Id,
            TransactionType = "execute",
            Status = execResult.Success ? "success" : "failed",
            ProviderReference = execResult.TransactionId,
            RawResponseSanitizedJson = execResult.RawResponseJson,
            CreatedAt = DateTimeOffset.UtcNow
        });

        if (execResult.Success)
        {
            payment.PaymentStatus = PaymentTransactionStatuses.Captured;
            payment.TransactionId = execResult.TransactionId;
            payment.CompletedAt = DateTimeOffset.UtcNow;

            if (payment.Order != null)
            {
                payment.Order.PaymentStatus = PaymentStatuses.Paid;
                if (payment.Order.OrderStatus == OrderStatuses.Pending)
                {
                    payment.Order.OrderStatus = OrderStatuses.Confirmed;
                }
                payment.Order.UpdatedAt = DateTimeOffset.UtcNow;

                _db.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = payment.Order.Id,
                    PreviousStatus = OrderStatuses.Pending,
                    NewStatus = OrderStatuses.Confirmed,
                    Notes = $"Payment received successfully via {providerCode}. TrxID: {execResult.TransactionId}",
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }
        else
        {
            payment.PaymentStatus = PaymentTransactionStatuses.Failed;
        }

        await _db.SaveChangesAsync(ct);

        return new PaymentResultDto(
            Success: execResult.Success,
            PaymentPublicId: payment.PublicId,
            OrderId: payment.OrderId,
            OrderNumber: payment.Order?.OrderNumber ?? "",
            PaymentStatus: payment.PaymentStatus,
            OrderStatus: payment.Order?.OrderStatus ?? "",
            TransactionId: payment.TransactionId,
            Amount: payment.Amount,
            Message: execResult.Success ? "Payment completed successfully." : (execResult.ErrorMessage ?? "Payment failed.")
        );
    }

    public async Task<RefundDto> ProcessRefundAsync(long adminUserId, ProcessRefundRequest request, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Payments)
            .ThenInclude(p => p.PaymentMethod)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        var payment = order.Payments.FirstOrDefault(p => p.PaymentStatus == PaymentTransactionStatuses.Captured);
        if (payment == null)
        {
            throw new BusinessRuleViolationException("No completed payment exists for this order to refund.");
        }

        if (request.Amount <= 0 || request.Amount > payment.Amount)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Amount", new[] { $"Refund amount must be between 0 and {payment.Amount} BDT." } }
            });
        }

        var providerCode = (payment.PaymentMethod?.Code ?? payment.GatewayProvider ?? "cod").ToLowerInvariant();
        var gateway = _gateways.FirstOrDefault(g => g.ProviderCode.Equals(providerCode, StringComparison.OrdinalIgnoreCase))
            ?? throw new BusinessRuleViolationException($"Payment provider '{providerCode}' is unavailable.");

        var refundResult = await gateway.ProcessRefundAsync(payment, request.Amount, request.Reason, ct);

        if (!refundResult.Success)
        {
            throw new BusinessRuleViolationException($"Gateway refund failed: {refundResult.ErrorMessage}");
        }

        var refund = new Refund
        {
            OrderId = order.Id,
            PaymentId = payment.Id,
            RefundAmount = request.Amount,
            Reason = request.Reason,
            Status = "processed",
            ProcessedBy = adminUserId,
            ProcessedAt = DateTimeOffset.UtcNow,
            MfsRefundTrxId = refundResult.RefundTransactionId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Refunds.Add(refund);

        if (request.Amount == payment.Amount)
        {
            payment.PaymentStatus = PaymentTransactionStatuses.Refunded;
            order.PaymentStatus = PaymentStatuses.Refunded;
            order.OrderStatus = OrderStatuses.Cancelled;
        }

        _db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            PreviousStatus = order.OrderStatus,
            NewStatus = order.OrderStatus,
            ChangedBy = adminUserId,
            Notes = $"Refund processed for {request.Amount:F2} BDT. TrxID: {refundResult.RefundTransactionId}. Reason: {request.Reason}",
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return new RefundDto(
            Id: refund.Id,
            OrderId: refund.OrderId,
            PaymentId: refund.PaymentId,
            RefundAmount: refund.RefundAmount,
            Reason: refund.Reason,
            Status: refund.Status,
            MfsRefundTrxId: refund.MfsRefundTrxId,
            CreatedAt: refund.CreatedAt
        );
    }
}
