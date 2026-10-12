namespace NiramoyHealth.Domain.Enums;

public static class RoleNames
{
    public const string SuperAdmin = "super_admin";
    public const string Admin = "admin";
    public const string Pharmacist = "pharmacist";
    public const string DeliveryAgent = "delivery_agent";
    public const string SupportAgent = "support_agent";
    public const string Customer = "customer";

    public static readonly string[] All = [SuperAdmin, Admin, Pharmacist, DeliveryAgent, SupportAgent, Customer];
}

public static class AccountStatuses
{
    public const string Active = "active";
    public const string Inactive = "inactive";
    public const string Suspended = "suspended";
    public const string Deactivated = "deactivated";
    public const string PendingVerification = "pending_verification";
}

public static class OrderStatuses
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string PrescriptionCheck = "prescription_check";
    public const string Processing = "processing";
    public const string Packed = "packed";
    public const string Shipped = "shipped";
    public const string OutForDelivery = "out_for_delivery";
    public const string Delivered = "delivered";
    public const string Cancelled = "cancelled";
    public const string Returned = "returned";
}

public static class PaymentStatuses
{
    public const string Unpaid = "unpaid";
    public const string PartiallyPaid = "partially_paid";
    public const string Paid = "paid";
    public const string Refunded = "refunded";
    public const string Failed = "failed";
}

public static class PaymentTransactionStatuses
{
    public const string Pending = "pending";
    public const string Authorized = "authorized";
    public const string Captured = "captured";
    public const string Failed = "failed";
    public const string Refunded = "refunded";
    public const string Cancelled = "cancelled";
}

public static class PrescriptionStatuses
{
    public const string Submitted = "submitted";
    public const string UnderReview = "under_review";
    public const string Approved = "approved";
    public const string PartiallyApproved = "partially_approved";
    public const string Rejected = "rejected";
    public const string Expired = "expired";
}

public static class PaymentMethodCodes
{
    public const string Cod = "cod";
    public const string Bkash = "bkash";
    public const string Nagad = "nagad";
    public const string Rocket = "rocket";
    public const string CardSslCommerz = "card_sslcommerz";
}

public static class DeliveryMethodCodes
{
    public const string StandardDhaka = "standard_dhaka";
    public const string ExpressDhaka = "express_dhaka";
    public const string UrgentMedicine = "urgent_medicine";
    public const string OutsideDhaka = "outside_dhaka";
}

public static class DiscountTypes
{
    public const string Percentage = "percentage";
    public const string FixedAmount = "fixed_amount";
}

public static class CartStatuses
{
    public const string Active = "active";
    public const string Converted = "converted";
    public const string Abandoned = "abandoned";
}

public static class ReviewStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}

public static class SupportTicketPriorities
{
    public const string Low = "low";
    public const string Medium = "medium";
    public const string High = "high";
    public const string Urgent = "urgent";
}

public static class SupportTicketStatuses
{
    public const string Open = "open";
    public const string InProgress = "in_progress";
    public const string Resolved = "resolved";
    public const string Closed = "closed";
}
