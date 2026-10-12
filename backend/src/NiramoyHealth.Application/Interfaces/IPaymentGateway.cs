using NiramoyHealth.Domain.Entities;

namespace NiramoyHealth.Application.Interfaces;

public record PaymentGatewayInitResult(
    bool Success,
    string? GatewayPaymentId,
    string? RedirectUrl,
    string? ErrorMessage,
    string RawResponseJson
);

public record PaymentGatewayExecutionResult(
    bool Success,
    string? TransactionId,
    string? ErrorMessage,
    string RawResponseJson
);

public record PaymentGatewayRefundResult(
    bool Success,
    string? RefundTransactionId,
    string? ErrorMessage,
    string RawResponseJson
);

public interface IPaymentGateway
{
    string ProviderCode { get; }
    Task<PaymentGatewayInitResult> InitializePaymentAsync(Payment payment, Order order, CancellationToken ct = default);
    Task<PaymentGatewayExecutionResult> ExecutePaymentAsync(Payment payment, string? gatewayPaymentId, string? trxId, CancellationToken ct = default);
    Task<PaymentGatewayRefundResult> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken ct = default);
}
