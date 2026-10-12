namespace NiramoyHealth.Application.DTOs;

public record InitiatePaymentRequest(
    long OrderId
);

public record PaymentInitResultDto(
    Guid PaymentPublicId,
    long OrderId,
    string OrderNumber,
    string GatewayProvider,
    decimal Amount,
    string Currency,
    string? GatewayPaymentId,
    string? RedirectUrl,
    string Status
);

public record ExecutePaymentRequest(
    Guid PaymentPublicId,
    string? GatewayPaymentId,
    string? ProviderTrxId
);

public record PaymentResultDto(
    bool Success,
    Guid PaymentPublicId,
    long OrderId,
    string OrderNumber,
    string PaymentStatus,
    string OrderStatus,
    string? TransactionId,
    decimal Amount,
    string Message
);

public record ProcessRefundRequest(
    long OrderId,
    decimal Amount,
    string Reason
);

public record RefundDto(
    long Id,
    long OrderId,
    long PaymentId,
    decimal RefundAmount,
    string Reason,
    string Status,
    string? MfsRefundTrxId,
    DateTimeOffset CreatedAt
);
