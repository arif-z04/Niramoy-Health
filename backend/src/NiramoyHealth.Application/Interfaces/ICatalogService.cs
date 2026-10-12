using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public interface ICatalogService
{
    Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductFilterParams filter, CancellationToken ct = default);
    Task<ProductDetailDto> GetProductBySlugAsync(string slug, CancellationToken ct = default);
    Task<ProductDetailDto> GetProductByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<CategoryDto> GetCategoryBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<BrandDto>> GetBrandsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GenericDto>> GetGenericsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DosageFormDto>> GetDosageFormsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProductListItemDto>> GetFeaturedProductsAsync(int count, CancellationToken ct = default);
    Task<IReadOnlyList<ProductListItemDto>> GetDiscountDealsAsync(int count, CancellationToken ct = default);
    Task<IReadOnlyList<ProductReviewDto>> GetProductReviewsAsync(long productId, CancellationToken ct = default);
    Task<ProductReviewDto> AddProductReviewAsync(long userId, long productId, CreateProductReviewRequest request, CancellationToken ct = default);
}
