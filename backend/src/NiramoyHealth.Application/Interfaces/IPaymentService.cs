using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentInitResultDto> InitiatePaymentAsync(long userId, long orderId, CancellationToken ct = default);
    Task<PaymentResultDto> ExecutePaymentAsync(ExecutePaymentRequest request, CancellationToken ct = default);
    Task<RefundDto> ProcessRefundAsync(long adminUserId, ProcessRefundRequest request, CancellationToken ct = default);
}
