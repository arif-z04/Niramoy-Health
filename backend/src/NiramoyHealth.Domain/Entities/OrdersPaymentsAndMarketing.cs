using NiramoyHealth.Domain.Common;

namespace NiramoyHealth.Domain.Entities;

public class DeliveryMethod : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public decimal BaseFee { get; set; }
    public decimal? FreeDeliveryThreshold { get; set; }
    public int EstimatedDaysMin { get; set; }
    public int EstimatedDaysMax { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class PaymentMethod : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public bool IsMfs { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Order : BaseAuditableEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;
    public long UserId { get; set; }
    public long? PrescriptionId { get; set; }
    public string OrderStatus { get; set; } = "pending";
    public string PaymentStatus { get; set; } = "unpaid";
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "BDT";
    public string? CouponCode { get; set; }
    public int DeliveryMethodId { get; set; }
    public int PaymentMethodId { get; set; }
    public string? CustomerNotes { get; set; }
    public string? CancelReason { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTimeOffset PlacedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
    public Prescription? Prescription { get; set; }
    public DeliveryMethod? DeliveryMethod { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public OrderAddress? DeliveryAddress { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}

public class OrderAddress : BaseEntity
{
    public long OrderId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string? Upazila { get; set; }
    public string AreaDetails { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? DeliveryInstructions { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Order? Order { get; set; }
}

public class OrderItem : BaseEntity
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long? ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductNameBn { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsPrescriptionItem { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}

public class OrderStatusHistory : BaseEntity
{
    public long OrderId { get; set; }
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public long? ChangedBy { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Order? Order { get; set; }
}

public class Payment : BaseEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public long OrderId { get; set; }
    public int PaymentMethodId { get; set; }
    public string PaymentStatus { get; set; } = "pending";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public string? TransactionId { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? GatewayProvider { get; set; }
    public string GatewayResponseJson { get; set; } = "{}";
    public DateTimeOffset InitiatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Order? Order { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}

public class PaymentTransaction : BaseEntity
{
    public long PaymentId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ProviderReference { get; set; }
    public string RawResponseSanitizedJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Payment? Payment { get; set; }
}

public class Refund : BaseEntity
{
    public long OrderId { get; set; }
    public long PaymentId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "requested";
    public long? ProcessedBy { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? MfsRefundTrxId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Order? Order { get; set; }
    public Payment? Payment { get; set; }
}

public class DeliveryZone : BaseEntity<int>
{
    public int DivisionId { get; set; }
    public int? DistrictId { get; set; }
    public string ZoneName { get; set; } = string.Empty;
    public int EstimatedDeliveryHours { get; set; }
    public decimal BaseDeliveryFee { get; set; }
    public bool IsExpressAvailable { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Division? Division { get; set; }
    public District? District { get; set; }
}

public class Coupon : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "percentage";
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int UsageLimitPerUser { get; set; } = 1;
    public int? TotalUsageLimit { get; set; }
    public int TimesUsed { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
}

public class CouponUsage : BaseEntity
{
    public int CouponId { get; set; }
    public long UserId { get; set; }
    public long OrderId { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTimeOffset UsedAt { get; set; } = DateTimeOffset.UtcNow;

    public Coupon? Coupon { get; set; }
    public User? User { get; set; }
    public Order? Order { get; set; }
}

public class PromotionalCampaign : BaseEntity<int>
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string? Description { get; set; }
    public string? BannerUrl { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Banner> Banners { get; set; } = new List<Banner>();
}

public class Banner : BaseEntity<int>
{
    public int? CampaignId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public PromotionalCampaign? Campaign { get; set; }
}

public class HomepageSection : BaseEntity<int>
{
    public string SectionType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string ConfigurationJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class NewsletterSubscriber : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset SubscribedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Notification : BaseEntity
{
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleBn { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageBn { get; set; }
    public string NotificationType { get; set; } = "system";
    public string? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}

public class NotificationPreference
{
    public long UserId { get; set; }
    public bool SmsOrderUpdates { get; set; } = true;
    public bool SmsPromotions { get; set; }
    public bool EmailOrderUpdates { get; set; } = true;
    public bool EmailPromotions { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}

public class SupportTicket : BaseAuditableEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public long UserId { get; set; }
    public long? OrderId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Priority { get; set; } = "medium";
    public string Status { get; set; } = "open";
    public long? AssignedTo { get; set; }

    public User? User { get; set; }
    public Order? Order { get; set; }
    public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
}

public class SupportMessage : BaseEntity
{
    public long TicketId { get; set; }
    public long SenderId { get; set; }
    public bool IsStaffReply { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public SupportTicket? Ticket { get; set; }
    public User? Sender { get; set; }
}

public class AuditLog : BaseEntity
{
    public long? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class AdministrativeAction : BaseEntity
{
    public long AdminUserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public long? TargetUserId { get; set; }
    public long? TargetOrderId { get; set; }
    public long? TargetProductId { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? AdminUser { get; set; }
}

public class AppConfiguration : BaseEntity<int>
{
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
