using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly INiramoyDbContext _db;

    public CatalogService(INiramoyDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductFilterParams filter, CancellationToken ct = default)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Generic)
            .Include(p => p.DosageForm)
            .Include(p => p.Images)
            .Include(p => p.Inventories)
            .Where(p => p.IsActive && p.DeletedAt == null);

        // Search
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%") ||
                EF.Functions.ILike(p.NameBn, $"%{term}%") ||
                EF.Functions.ILike(p.Sku, $"%{term}%") ||
                (p.Generic != null && EF.Functions.ILike(p.Generic.Name, $"%{term}%")) ||
                (p.Brand != null && EF.Functions.ILike(p.Brand.Name, $"%{term}%")));
        }

        // Category filter (support category slug or id)
        if (filter.CategoryId.HasValue)
        {
            var catId = filter.CategoryId.Value;
            var subCatIds = await _db.Categories
                .Where(c => c.ParentId == catId || c.Id == catId)
                .Select(c => c.Id)
                .ToListAsync(ct);

            query = query.Where(p => subCatIds.Contains(p.CategoryId));
        }
        else if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            var parentCat = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == filter.CategorySlug, ct);
            if (parentCat != null)
            {
                var subCatIds = await _db.Categories
                    .Where(c => c.ParentId == parentCat.Id || c.Id == parentCat.Id)
                    .Select(c => c.Id)
                    .ToListAsync(ct);

                query = query.Where(p => subCatIds.Contains(p.CategoryId));
            }
        }

        if (filter.BrandId.HasValue)
            query = query.Where(p => p.BrandId == filter.BrandId.Value);

        if (filter.GenericId.HasValue)
            query = query.Where(p => p.GenericId == filter.GenericId.Value);

        if (filter.ManufacturerId.HasValue)
            query = query.Where(p => p.ManufacturerId == filter.ManufacturerId.Value);

        if (filter.DosageFormId.HasValue)
            query = query.Where(p => p.DosageFormId == filter.DosageFormId.Value);

        if (filter.IsPrescriptionRequired.HasValue)
            query = query.Where(p => p.IsPrescriptionRequired == filter.IsPrescriptionRequired.Value);

        if (filter.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => (p.DiscountPrice ?? p.BasePrice) >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => (p.DiscountPrice ?? p.BasePrice) <= filter.MaxPrice.Value);

        if (filter.InStockOnly == true)
            query = query.Where(p => p.Inventories.Sum(i => i.QuantityOnHand - i.QuantityReserved) > 0);

        // Sorting
        query = filter.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.BasePrice),
            "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.BasePrice),
            "rating" => query.OrderByDescending(p => p.AvgRating).ThenByDescending(p => p.ReviewCount),
            "popularity" => query.OrderByDescending(p => p.ReviewCount).ThenByDescending(p => p.AvgRating),
            "name_asc" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var page = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToListItemDto).ToList();
        return new PagedResult<ProductListItemDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<ProductDetailDto> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Manufacturer)
            .Include(p => p.Generic)
            .Include(p => p.DosageForm)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Attributes)
            .Include(p => p.Reviews.Where(r => r.Status == "approved"))
            .ThenInclude(r => r.User)
            .Include(p => p.Inventories)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive && p.DeletedAt == null, ct);

        if (product == null)
            throw new NotFoundException(nameof(Product), slug);

        return await BuildProductDetailDtoAsync(product, ct);
    }

    public async Task<ProductDetailDto> GetProductByIdAsync(long id, CancellationToken ct = default)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Manufacturer)
            .Include(p => p.Generic)
            .Include(p => p.DosageForm)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Attributes)
            .Include(p => p.Reviews.Where(r => r.Status == "approved"))
            .ThenInclude(r => r.User)
            .Include(p => p.Inventories)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive && p.DeletedAt == null, ct);

        if (product == null)
            throw new NotFoundException(nameof(Product), id);

        return await BuildProductDetailDtoAsync(product, ct);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var allCategories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        var topCategories = allCategories.Where(c => c.ParentId == null).ToList();
        var result = new List<CategoryDto>();

        foreach (var top in topCategories)
        {
            var subs = allCategories
                .Where(c => c.ParentId == top.Id)
                .Select(c => new CategoryDto(
                    c.Id, c.Slug, c.Name, c.NameBn, c.Description, c.IconUrl, c.BannerUrl, c.DisplayOrder, c.ParentId
                ))
                .ToList();

            result.Add(new CategoryDto(
                top.Id, top.Slug, top.Name, top.NameBn, top.Description, top.IconUrl, top.BannerUrl, top.DisplayOrder, null, subs
            ));
        }

        return result;
    }

    public async Task<CategoryDto> GetCategoryBySlugAsync(string slug, CancellationToken ct = default)
    {
        var category = await _db.Categories
            .Include(c => c.SubCategories.Where(s => s.IsActive))
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive, ct);

        if (category == null)
            throw new NotFoundException(nameof(Category), slug);

        var subDtos = category.SubCategories
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new CategoryDto(s.Id, s.Slug, s.Name, s.NameBn, s.Description, s.IconUrl, s.BannerUrl, s.DisplayOrder, s.ParentId))
            .ToList();

        return new CategoryDto(category.Id, category.Slug, category.Name, category.NameBn, category.Description, category.IconUrl, category.BannerUrl, category.DisplayOrder, category.ParentId, subDtos);
    }

    public async Task<IReadOnlyList<BrandDto>> GetBrandsAsync(CancellationToken ct = default)
    {
        var brands = await _db.Brands
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);

        return brands.Select(b => new BrandDto(b.Id, b.Slug, b.Name, b.NameBn, b.LogoUrl, b.Description, b.WebsiteUrl)).ToList();
    }

    public async Task<IReadOnlyList<GenericDto>> GetGenericsAsync(CancellationToken ct = default)
    {
        var generics = await _db.Generics
            .AsNoTracking()
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .ToListAsync(ct);

        return generics.Select(g => new GenericDto(
            g.Id, g.Slug, g.Name, g.NameBn, g.Description, g.TherapeuticClass, g.Indication, g.DosageGuidelines, g.SideEffects, g.Precautions, g.PregnancyCategory
        )).ToList();
    }

    public async Task<IReadOnlyList<DosageFormDto>> GetDosageFormsAsync(CancellationToken ct = default)
    {
        var forms = await _db.DosageForms
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        return forms.Select(d => new DosageFormDto(d.Id, d.Name, d.NameBn)).ToList();
    }

    public async Task<IReadOnlyList<ProductListItemDto>> GetFeaturedProductsAsync(int count, CancellationToken ct = default)
    {
        var limit = Math.Clamp(count, 1, 50);
        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Generic)
            .Include(p => p.DosageForm)
            .Include(p => p.Images)
            .Include(p => p.Inventories)
            .Where(p => p.IsActive && p.IsFeatured && p.DeletedAt == null)
            .OrderByDescending(p => p.AvgRating)
            .ThenByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        return products.Select(MapToListItemDto).ToList();
    }

    public async Task<IReadOnlyList<ProductListItemDto>> GetDiscountDealsAsync(int count, CancellationToken ct = default)
    {
        var limit = Math.Clamp(count, 1, 50);
        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Generic)
            .Include(p => p.DosageForm)
            .Include(p => p.Images)
            .Include(p => p.Inventories)
            .Where(p => p.IsActive && p.DeletedAt == null && p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.BasePrice)
            .OrderByDescending(p => (p.BasePrice - p.DiscountPrice!.Value) / p.BasePrice)
            .Take(limit)
            .ToListAsync(ct);

        return products.Select(MapToListItemDto).ToList();
    }

    public async Task<IReadOnlyList<ProductReviewDto>> GetProductReviewsAsync(long productId, CancellationToken ct = default)
    {
        var reviews = await _db.ProductReviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.Status == "approved")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return reviews.Select(r => new ProductReviewDto(
            Id: r.Id,
            UserId: r.UserId,
            UserName: r.User?.FullName ?? "Customer",
            Rating: r.Rating,
            Title: r.Title,
            Comment: r.Comment,
            IsVerifiedPurchase: r.IsVerifiedPurchase,
            CreatedAt: r.CreatedAt
        )).ToList();
    }

    public async Task<ProductReviewDto> AddProductReviewAsync(long userId, long productId, CreateProductReviewRequest request, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive && p.DeletedAt == null, ct)
            ?? throw new NotFoundException(nameof(Product), productId);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        var isVerified = await _db.OrderItems
            .AnyAsync(oi => oi.ProductId == productId && oi.Order != null && oi.Order.UserId == userId && oi.Order.OrderStatus == "delivered", ct);

        var review = new ProductReview
        {
            ProductId = productId,
            UserId = userId,
            Rating = Math.Clamp(request.Rating, 1, 5),
            Title = request.Title,
            Comment = request.Comment,
            IsVerifiedPurchase = isVerified,
            Status = "approved", // auto-approve for storefront simplicity or "pending" if moderation flag
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.ProductReviews.Add(review);
        await _db.SaveChangesAsync(ct);

        // Update product average rating and count
        var stats = await _db.ProductReviews
            .Where(r => r.ProductId == productId && r.Status == "approved")
            .GroupBy(r => r.ProductId)
            .Select(g => new { Avg = (decimal)g.Average(r => r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        if (stats != null)
        {
            product.AvgRating = Math.Round(stats.Avg, 2);
            product.ReviewCount = stats.Count;
            await _db.SaveChangesAsync(ct);
        }

        return new ProductReviewDto(
            Id: review.Id,
            UserId: user.Id,
            UserName: user.FullName,
            Rating: review.Rating,
            Title: review.Title,
            Comment: review.Comment,
            IsVerifiedPurchase: review.IsVerifiedPurchase,
            CreatedAt: review.CreatedAt
        );
    }

    private async Task<ProductDetailDto> BuildProductDetailDtoAsync(Product product, CancellationToken ct)
    {
        var availableStock = product.Inventories.Sum(i => i.QuantityOnHand - i.QuantityReserved);

        var images = product.Images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.AltText, i.DisplayOrder, i.IsPrimary))
            .ToList();

        var variants = product.Variants
            .Where(v => v.IsActive)
            .Select(v => new ProductVariantDto(
                v.Id, v.Sku, v.Name, v.NameBn, v.BasePrice, v.DiscountPrice,
                v.DiscountPrice ?? v.BasePrice, v.VariantAttributesJson
            ))
            .ToList();

        var attributes = product.Attributes
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new ProductAttributeDto(a.Id, a.AttributeKey, a.AttributeValue, a.AttributeValueBn, a.DisplayOrder))
            .ToList();

        var reviews = product.Reviews
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ProductReviewDto(
                r.Id, r.UserId, r.User?.FullName ?? "Customer", r.Rating, r.Title, r.Comment, r.IsVerifiedPurchase, r.CreatedAt
            ))
            .ToList();

        // Related products in the same category
        var relatedProducts = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Generic)
            .Include(p => p.DosageForm)
            .Include(p => p.Images)
            .Include(p => p.Inventories)
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive && p.DeletedAt == null)
            .OrderByDescending(p => p.AvgRating)
            .Take(4)
            .ToListAsync(ct);

        var currentPrice = product.DiscountPrice ?? product.BasePrice;
        var discountPct = product.DiscountPrice.HasValue && product.BasePrice > 0
            ? Math.Round(((product.BasePrice - product.DiscountPrice.Value) / product.BasePrice) * 100, 2)
            : 0m;

        return new ProductDetailDto(
            Id: product.Id,
            PublicId: product.PublicId,
            Sku: product.Sku,
            Slug: product.Slug,
            Name: product.Name,
            NameBn: product.NameBn,
            CategoryId: product.CategoryId,
            CategoryName: product.Category?.Name ?? "",
            BrandName: product.Brand?.Name,
            GenericName: product.Generic?.Name,
            DosageFormName: product.DosageForm?.Name,
            ManufacturerName: product.Manufacturer?.Name,
            Strength: product.Strength,
            PackSize: product.PackSize,
            UnitOfMeasure: product.UnitOfMeasure,
            UnitsPerPack: product.UnitsPerPack,
            BasePrice: product.BasePrice,
            DiscountPrice: product.DiscountPrice,
            CurrentPrice: currentPrice,
            DiscountPercentage: discountPct,
            IsPrescriptionRequired: product.IsPrescriptionRequired,
            IsOtc: product.IsOtc,
            Description: product.Description,
            DescriptionBn: product.DescriptionBn,
            UsageInstructions: product.UsageInstructions,
            StorageConditions: product.StorageConditions,
            IsFeatured: product.IsFeatured,
            Badge: product.Badge,
            AvgRating: product.AvgRating,
            ReviewCount: product.ReviewCount,
            IsInStock: availableStock > 0,
            StockQuantity: availableStock,
            Images: images,
            Variants: variants,
            Attributes: attributes,
            Reviews: reviews,
            RelatedProducts: relatedProducts.Select(MapToListItemDto).ToList()
        );
    }

    private static ProductListItemDto MapToListItemDto(Product p)
    {
        var availableStock = p.Inventories.Sum(i => i.QuantityOnHand - i.QuantityReserved);
        var currentPrice = p.DiscountPrice ?? p.BasePrice;
        var discountPct = p.DiscountPrice.HasValue && p.BasePrice > 0
            ? Math.Round(((p.BasePrice - p.DiscountPrice.Value) / p.BasePrice) * 100, 2)
            : 0m;

        var primaryImage = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
            ?? p.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;

        return new ProductListItemDto(
            Id: p.Id,
            PublicId: p.PublicId,
            Sku: p.Sku,
            Slug: p.Slug,
            Name: p.Name,
            NameBn: p.NameBn,
            CategoryId: p.CategoryId,
            CategoryName: p.Category?.Name ?? "",
            BrandName: p.Brand?.Name,
            GenericName: p.Generic?.Name,
            DosageFormName: p.DosageForm?.Name,
            Strength: p.Strength,
            PackSize: p.PackSize,
            UnitOfMeasure: p.UnitOfMeasure,
            BasePrice: p.BasePrice,
            DiscountPrice: p.DiscountPrice,
            CurrentPrice: currentPrice,
            DiscountPercentage: discountPct,
            IsPrescriptionRequired: p.IsPrescriptionRequired,
            IsOtc: p.IsOtc,
            IsFeatured: p.IsFeatured,
            Badge: p.Badge,
            AvgRating: p.AvgRating,
            ReviewCount: p.ReviewCount,
            PrimaryImageUrl: primaryImage,
            IsInStock: availableStock > 0,
            StockQuantity: availableStock
        );
    }
}
