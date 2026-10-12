namespace NiramoyHealth.Application.DTOs;

public record OrderAddressInput(
    string RecipientName,
    string RecipientPhone,
    string Division,
    string District,
    string? Upazila,
    string AreaDetails,
    string? PostalCode,
    string? DeliveryInstructions
);

public record CheckoutRequest(
    long? DeliveryAddressId,
    OrderAddressInput? NewAddress,
    int DeliveryMethodId,
    int PaymentMethodId,
    long? PrescriptionId,
    string? CouponCode,
    string? CustomerNotes,
    string? IdempotencyKey,
    long? AddressId = null
)
{
    public long? EffectiveAddressId => DeliveryAddressId ?? AddressId;
};

public record OrderAddressDto(
    string RecipientName,
    string RecipientPhone,
    string Division,
    string District,
    string? Upazila,
    string AreaDetails,
    string? PostalCode,
    string? DeliveryInstructions
);

public record OrderItemDto(
    long Id,
    long ProductId,
    long? ProductVariantId,
    string ProductName,
    string ProductNameBn,
    string ProductSku,
    decimal UnitPriceSnapshot,
    int Quantity,
    decimal TotalPrice,
    bool IsPrescriptionItem,
    string? ProductImageUrl
);

public record OrderStatusHistoryDto(
    long Id,
    string? PreviousStatus,
    string NewStatus,
    string? ChangedByName,
    string? Notes,
    DateTimeOffset CreatedAt
);

public record PaymentSummaryDto(
    long Id,
    Guid PublicId,
    string PaymentMethodCode,
    string PaymentMethodName,
    string PaymentStatus,
    decimal Amount,
    string Currency,
    string? TransactionId,
    string? GatewayProvider,
    DateTimeOffset InitiatedAt,
    DateTimeOffset? CompletedAt
);

public record OrderDto(
    long Id,
    Guid PublicId,
    string OrderNumber,
    long UserId,
    string CustomerName,
    string CustomerPhone,
    long? PrescriptionId,
    string OrderStatus,
    string PaymentStatus,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal DeliveryFee,
    decimal TotalAmount,
    string Currency,
    string? CouponCode,
    int DeliveryMethodId,
    string DeliveryMethodName,
    int PaymentMethodId,
    string PaymentMethodCode,
    string PaymentMethodName,
    string? CustomerNotes,
    string? CancelReason,
    DateTimeOffset PlacedAt,
    OrderAddressDto? DeliveryAddress,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory,
    IReadOnlyList<PaymentSummaryDto> Payments
);

public record OrderListSummaryDto(
    long Id,
    Guid PublicId,
    string OrderNumber,
    long UserId,
    string CustomerName,
    string CustomerPhone,
    string OrderStatus,
    string PaymentStatus,
    decimal TotalAmount,
    int ItemCount,
    string PaymentMethodName,
    DateTimeOffset PlacedAt
);

public class OrderFilterParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public long? UserId { get; set; }
    public string? OrderNumber { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
}

public record UpdateOrderStatusRequest(
    string NewStatus,
    string? Notes
);

public record CancelOrderRequest(
    string Reason
);

public record DeliveryMethodDto(
    int Id,
    string Code,
    string Name,
    string NameBn,
    decimal BaseFee,
    decimal? FreeDeliveryThreshold,
    int EstimatedDaysMin,
    int EstimatedDaysMax
);

public record PaymentMethodDto(
    int Id,
    string Code,
    string Name,
    string NameBn,
    bool IsMfs
);
