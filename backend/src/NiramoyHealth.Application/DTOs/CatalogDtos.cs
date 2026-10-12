namespace NiramoyHealth.Application.DTOs;

public record CategoryDto(
    int Id,
    string Slug,
    string Name,
    string NameBn,
    string? Description,
    string? IconUrl,
    string? BannerUrl,
    int DisplayOrder,
    int? ParentId,
    IReadOnlyList<CategoryDto>? SubCategories = null
);

public record CreateCategoryRequest(
    string Name,
    string NameBn,
    string Slug,
    string? Description,
    string? IconUrl,
    string? BannerUrl,
    int DisplayOrder,
    int? ParentId,
    bool IsActive = true
);

public record UpdateCategoryRequest(
    string Name,
    string NameBn,
    string Slug,
    string? Description,
    string? IconUrl,
    string? BannerUrl,
    int DisplayOrder,
    int? ParentId,
    bool IsActive
);

public record BrandDto(
    int Id,
    string Slug,
    string Name,
    string? NameBn,
    string? LogoUrl,
    string? Description,
    string? WebsiteUrl
);

public record ManufacturerDto(
    int Id,
    string Slug,
    string Name,
    string? NameBn,
    string Country,
    string? LicenseNumber,
    string? WebsiteUrl
);

public record GenericDto(
    int Id,
    string Slug,
    string Name,
    string? NameBn,
    string? Description,
    string? TherapeuticClass,
    string? Indication,
    string? DosageGuidelines,
    string? SideEffects,
    string? Precautions,
    string? PregnancyCategory
);

public record DosageFormDto(
    int Id,
    string Name,
    string? NameBn
);

public record ProductImageDto(
    long Id,
    string ImageUrl,
    string? AltText,
    int DisplayOrder,
    bool IsPrimary
);

public record ProductVariantDto(
    long Id,
    string Sku,
    string Name,
    string? NameBn,
    decimal BasePrice,
    decimal? DiscountPrice,
    decimal CurrentPrice,
    string VariantAttributesJson
);

public record ProductAttributeDto(
    long Id,
    string AttributeKey,
    string AttributeValue,
    string? AttributeValueBn,
    int DisplayOrder
);

public record ProductReviewDto(
    long Id,
    long UserId,
    string UserName,
    int Rating,
    string? Title,
    string? Comment,
    bool IsVerifiedPurchase,
    DateTimeOffset CreatedAt
);

public record CreateProductReviewRequest(
    int Rating,
    string? Title,
    string? Comment
);

public record ProductListItemDto(
    long Id,
    Guid PublicId,
    string Sku,
    string Slug,
    string Name,
    string NameBn,
    int CategoryId,
    string CategoryName,
    string? BrandName,
    string? GenericName,
    string? DosageFormName,
    string? Strength,
    string? PackSize,
    string UnitOfMeasure,
    decimal BasePrice,
    decimal? DiscountPrice,
    decimal CurrentPrice,
    decimal DiscountPercentage,
    bool IsPrescriptionRequired,
    bool IsOtc,
    bool IsFeatured,
    string? Badge,
    decimal AvgRating,
    int ReviewCount,
    string? PrimaryImageUrl,
    bool IsInStock,
    int StockQuantity
);

public record ProductDetailDto(
    long Id,
    Guid PublicId,
    string Sku,
    string Slug,
    string Name,
    string NameBn,
    int CategoryId,
    string CategoryName,
    string? BrandName,
    string? GenericName,
    string? DosageFormName,
    string? ManufacturerName,
    string? Strength,
    string? PackSize,
    string UnitOfMeasure,
    int UnitsPerPack,
    decimal BasePrice,
    decimal? DiscountPrice,
    decimal CurrentPrice,
    decimal DiscountPercentage,
    bool IsPrescriptionRequired,
    bool IsOtc,
    string? Description,
    string? DescriptionBn,
    string? UsageInstructions,
    string? StorageConditions,
    bool IsFeatured,
    string? Badge,
    decimal AvgRating,
    int ReviewCount,
    bool IsInStock,
    int StockQuantity,
    IReadOnlyList<ProductImageDto> Images,
    IReadOnlyList<ProductVariantDto> Variants,
    IReadOnlyList<ProductAttributeDto> Attributes,
    IReadOnlyList<ProductReviewDto> Reviews,
    IReadOnlyList<ProductListItemDto> RelatedProducts
);

public class ProductFilterParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? CategorySlug { get; set; }
    public int? BrandId { get; set; }
    public int? GenericId { get; set; }
    public int? ManufacturerId { get; set; }
    public int? DosageFormId { get; set; }
    public bool? IsPrescriptionRequired { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public bool? IsFeatured { get; set; }
    public string? SortBy { get; set; } // "price_asc", "price_desc", "rating", "newest", "name_asc", "popularity"
}

public record CreateProductRequest(
    string Sku,
    string Slug,
    string Name,
    string NameBn,
    int CategoryId,
    int? BrandId,
    int? ManufacturerId,
    int? GenericId,
    int? DosageFormId,
    string? Strength,
    string? PackSize,
    string UnitOfMeasure,
    int UnitsPerPack,
    decimal BasePrice,
    decimal? DiscountPrice,
    bool IsPrescriptionRequired,
    bool IsOtc,
    string? Description,
    string? DescriptionBn,
    string? UsageInstructions,
    string? StorageConditions,
    bool IsFeatured,
    string? Badge,
    int InitialStock = 0,
    int InitialWarehouseId = 1
);

public record UpdateProductRequest(
    string Name,
    string NameBn,
    int CategoryId,
    int? BrandId,
    int? ManufacturerId,
    int? GenericId,
    int? DosageFormId,
    string? Strength,
    string? PackSize,
    string UnitOfMeasure,
    int UnitsPerPack,
    decimal BasePrice,
    decimal? DiscountPrice,
    bool IsPrescriptionRequired,
    bool IsOtc,
    string? Description,
    string? DescriptionBn,
    string? UsageInstructions,
    string? StorageConditions,
    bool IsFeatured,
    string? Badge,
    bool IsActive
);
