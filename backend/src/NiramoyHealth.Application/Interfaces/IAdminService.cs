using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public interface IAdminService
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(int pageNumber, int pageSize, string? search, CancellationToken ct = default);
    Task UpdateUserStatusAsync(long adminUserId, long targetUserId, UpdateUserStatusRequest request, CancellationToken ct = default);
    Task<PagedResult<WarehouseInventoryDto>> GetInventoryListAsync(int? warehouseId, bool lowStockOnly, int pageNumber, int pageSize, CancellationToken ct = default);
    Task AdjustStockAsync(long adminUserId, StockAdjustmentRequest request, CancellationToken ct = default);
    Task<ProductDetailDto> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDetailDto> UpdateProductAsync(long productId, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteProductAsync(long productId, CancellationToken ct = default);
    Task<PagedResult<CouponDto>> GetCouponsAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<CouponDto> CreateCouponAsync(CreateCouponRequest request, CancellationToken ct = default);
    Task<CouponDto> UpdateCouponAsync(int couponId, UpdateCouponRequest request, CancellationToken ct = default);
    Task DeleteCouponAsync(int couponId, CancellationToken ct = default);
}
