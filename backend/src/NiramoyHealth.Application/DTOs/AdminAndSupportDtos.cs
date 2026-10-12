namespace NiramoyHealth.Application.DTOs;

public record DashboardMetricsDto(
    decimal TotalGmv,
    decimal TodayGmv,
    int TotalOrders,
    int TodayOrders,
    int ActiveCustomersCount,
    int LowStockItemCount,
    int PendingPrescriptionsCount,
    Dictionary<string, int> OrdersByStatus,
    IReadOnlyList<OrderListSummaryDto> RecentOrders
);

public record WarehouseInventoryDto(
    long ProductId,
    string ProductName,
    string Sku,
    int WarehouseId,
    string WarehouseName,
    int QuantityOnHand,
    int QuantityReserved,
    int AvailableQuantity,
    int ReorderLevel,
    bool IsLowStock
);

public record StockAdjustmentRequest(
    long ProductId,
    int WarehouseId,
    int QuantityChange,
    string TransactionType, // "adjustment_add", "adjustment_subtract", "damage", "loss"
    string? Notes
);

public record AdminUserListItemDto(
    long Id,
    Guid PublicId,
    string PhoneNumber,
    string? Email,
    string FullName,
    string Status,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAt,
    int TotalOrders,
    decimal TotalSpent
);

public record UpdateUserStatusRequest(
    string Status, // "active", "suspended", "inactive"
    string? Reason
);

public record CouponDto(
    int Id,
    string Code,
    string Title,
    string? TitleBn,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    decimal MinOrderAmount,
    int UsageLimitPerUser,
    int? TotalUsageLimit,
    int TimesUsed,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidUntil,
    bool IsActive
);

public record CreateCouponRequest(
    string Code,
    string Title,
    string? TitleBn,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    decimal MinOrderAmount,
    int UsageLimitPerUser,
    int? TotalUsageLimit,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidUntil,
    bool IsActive = true
);

public record UpdateCouponRequest(
    string Title,
    string? TitleBn,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    decimal MinOrderAmount,
    int UsageLimitPerUser,
    int? TotalUsageLimit,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidUntil,
    bool IsActive
);

public record SupportMessageDto(
    long Id,
    long TicketId,
    long SenderId,
    string SenderName,
    bool IsStaffReply,
    string Message,
    DateTimeOffset CreatedAt
);

public record SupportTicketDto(
    long Id,
    string TicketNumber,
    long UserId,
    string UserName,
    string UserPhone,
    long? OrderId,
    string? OrderNumber,
    string Subject,
    string Priority,
    string Status,
    long? AssignedTo,
    DateTimeOffset CreatedAt,
    IReadOnlyList<SupportMessageDto> Messages
);

public record CreateSupportTicketRequest(
    long? OrderId,
    string Subject,
    string Priority,
    string InitialMessage
);

public record ReplySupportTicketRequest(
    string Message
);

public record UpdateSupportTicketStatusRequest(
    string Status
);

public record AuditLogDto(
    long Id,
    long? UserId,
    string Action,
    string EntityType,
    string EntityId,
    string? OldValuesJson,
    string? NewValuesJson,
    string? IpAddress,
    DateTimeOffset CreatedAt
);
