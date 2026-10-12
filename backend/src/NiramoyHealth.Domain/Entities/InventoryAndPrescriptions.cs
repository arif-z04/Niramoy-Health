using NiramoyHealth.Domain.Common;

namespace NiramoyHealth.Domain.Entities;

public class Warehouse : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DivisionId { get; set; }
    public int DistrictId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public bool IsCentralHub { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Division? Division { get; set; }
    public District? District { get; set; }
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}

public class Inventory : BaseEntity
{
    public long ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public int ReorderQuantity { get; set; } = 50;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}

public class MedicineBatch : BaseEntity
{
    public long ProductId { get; set; }
    public int WarehouseId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly MfgDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityRemaining { get; set; }
    public decimal UnitCost { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}

public class InventoryTransaction : BaseEntity
{
    public long ProductId { get; set; }
    public int WarehouseId { get; set; }
    public long? BatchId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public int BalanceAfter { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    public MedicineBatch? Batch { get; set; }
}

public class Cart : BaseAuditableEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public long? UserId { get; set; }
    public Guid? GuestToken { get; set; }
    public string Status { get; set; } = "active";
    public DateTimeOffset ExpiresAt { get; set; }

    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem : BaseEntity
{
    public long CartId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Cart? Cart { get; set; }
    public Product? Product { get; set; }
}

public class Wishlist : BaseAuditableEntity
{
    public long UserId { get; set; }
    public string Name { get; set; } = "My Wishlist";

    public User? User { get; set; }
    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}

public class WishlistItem : BaseEntity
{
    public long WishlistId { get; set; }
    public long ProductId { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;

    public Wishlist? Wishlist { get; set; }
    public Product? Product { get; set; }
}

public class Prescription : BaseAuditableEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public long UserId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int? PatientAge { get; set; }
    public string? PatientGender { get; set; }
    public string? DoctorName { get; set; }
    public string? HospitalClinic { get; set; }
    public DateOnly? PrescriptionDate { get; set; }
    public string Status { get; set; } = "submitted";
    public string? RejectionReason { get; set; }
    public string? CustomerNotes { get; set; }
    public long? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }

    public User? User { get; set; }
    public User? Reviewer { get; set; }
    public ICollection<PrescriptionFile> Files { get; set; } = new List<PrescriptionFile>();
    public ICollection<PrescriptionReviewRecord> ReviewRecords { get; set; } = new List<PrescriptionReviewRecord>();
}

public class PrescriptionFile : BaseEntity
{
    public long PrescriptionId { get; set; }
    public string StorageProvider { get; set; } = "local";
    public string FileStorageKey { get; set; } = string.Empty;
    public string OriginalFilename { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public bool IsSecureStorage { get; set; } = true;
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    public Prescription? Prescription { get; set; }
}

public class PrescriptionReviewRecord : BaseEntity
{
    public long PrescriptionId { get; set; }
    public long PharmacistId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string DecisionNotes { get; set; } = string.Empty;
    public string VerifiedMedicinesJson { get; set; } = "[]";
    public DateTimeOffset ReviewedAt { get; set; } = DateTimeOffset.UtcNow;

    public Prescription? Prescription { get; set; }
    public User? Pharmacist { get; set; }
}
