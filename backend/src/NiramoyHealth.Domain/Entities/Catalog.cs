using NiramoyHealth.Domain.Common;

namespace NiramoyHealth.Domain.Entities;

public class Category : BaseEntity<int>
{
    public int? ParentId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? BannerUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Category? Parent { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Brand : BaseEntity<int>
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Manufacturer : BaseEntity<int>
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public string Country { get; set; } = "Bangladesh";
    public string? LicenseNumber { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Generic : BaseEntity<int>
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public string? Description { get; set; }
    public string? TherapeuticClass { get; set; }
    public string? Indication { get; set; }
    public string? DosageGuidelines { get; set; }
    public string? SideEffects { get; set; }
    public string? Precautions { get; set; }
    public string? PregnancyCategory { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class DosageForm : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product : BaseAuditableEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string Sku { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    public int? ManufacturerId { get; set; }
    public int? GenericId { get; set; }
    public int? DosageFormId { get; set; }
    public string? Strength { get; set; }
    public string? PackSize { get; set; }
    public string UnitOfMeasure { get; set; } = "Piece";
    public int UnitsPerPack { get; set; } = 1;
    public decimal BasePrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public bool IsPrescriptionRequired { get; set; }
    public bool IsOtc { get; set; } = true;
    public string? Description { get; set; }
    public string? DescriptionBn { get; set; }
    public string? UsageInstructions { get; set; }
    public string? StorageConditions { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Badge { get; set; }
    public decimal AvgRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Category? Category { get; set; }
    public Brand? Brand { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public Generic? Generic { get; set; }
    public DosageForm? DosageForm { get; set; }

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}

public class ProductVariant : BaseEntity
{
    public long ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public string VariantAttributesJson { get; set; } = "{}";
    public decimal BasePrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Product? Product { get; set; }
}

public class ProductCategory
{
    public long ProductId { get; set; }
    public int CategoryId { get; set; }
    public bool IsPrimary { get; set; }

    public Product? Product { get; set; }
    public Category? Category { get; set; }
}

public class ProductImage : BaseEntity
{
    public long ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Product? Product { get; set; }
}

public class ProductAttribute : BaseEntity
{
    public long ProductId { get; set; }
    public string AttributeKey { get; set; } = string.Empty;
    public string AttributeValue { get; set; } = string.Empty;
    public string? AttributeValueBn { get; set; }
    public int DisplayOrder { get; set; }

    public Product? Product { get; set; }
}

public class ProductReview : BaseAuditableEntity
{
    public long ProductId { get; set; }
    public long UserId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public string Status { get; set; } = "pending";
    public long? ModeratedBy { get; set; }
    public DateTimeOffset? ModeratedAt { get; set; }
    public string? ModerationNote { get; set; }

    public Product? Product { get; set; }
    public User? User { get; set; }
}
