using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Enums;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class AdminService : IAdminService
{
    private readonly INiramoyDbContext _db;
    private readonly ICatalogService _catalogService;

    public AdminService(INiramoyDbContext db, ICatalogService catalogService)
    {
        _db = db;
        _catalogService = catalogService;
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync(CancellationToken ct = default)
    {
        var todayUtc = DateTime.UtcNow.Date;

        var nonCancelledOrders = _db.Orders.Where(o => o.OrderStatus != OrderStatuses.Cancelled);

        var totalGmv = await nonCancelledOrders.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;
        var todayGmv = await nonCancelledOrders.Where(o => o.PlacedAt >= todayUtc).SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;

        var totalOrders = await _db.Orders.CountAsync(ct);
        var todayOrders = await _db.Orders.Where(o => o.PlacedAt >= todayUtc).CountAsync(ct);

        var activeCustomers = await _db.Users
            .Where(u => u.Status == "active" && u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == RoleNames.Customer))
            .CountAsync(ct);

        var lowStockCount = await _db.Inventories
            .Where(i => (i.QuantityOnHand - i.QuantityReserved) <= i.ReorderLevel)
            .CountAsync(ct);

        var pendingPrescriptions = await _db.Prescriptions
            .Where(p => p.Status == PrescriptionStatuses.Submitted || p.Status == PrescriptionStatuses.UnderReview)
            .CountAsync(ct);

        var statusGroups = await _db.Orders
            .GroupBy(o => o.OrderStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var ordersByStatus = statusGroups.ToDictionary(g => g.Status, g => g.Count);

        var recentOrders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.PaymentMethod)
            .Include(o => o.Items)
            .OrderByDescending(o => o.PlacedAt)
            .Take(5)
            .Select(o => new OrderListSummaryDto(
                o.Id,
                o.PublicId,
                o.OrderNumber,
                o.UserId,
                o.User != null ? o.User.FullName : "Customer",
                o.User != null ? o.User.PhoneNumber : "",
                o.OrderStatus,
                o.PaymentStatus,
                o.TotalAmount,
                o.Items.Count,
                o.PaymentMethod != null ? o.PaymentMethod.Name : "Cash",
                o.PlacedAt
            ))
            .ToListAsync(ct);

        return new DashboardMetricsDto(
            TotalGmv: Math.Round(totalGmv, 2),
            TodayGmv: Math.Round(todayGmv, 2),
            TotalOrders: totalOrders,
            TodayOrders: todayOrders,
            ActiveCustomersCount: activeCustomers,
            LowStockItemCount: lowStockCount,
            PendingPrescriptionsCount: pendingPrescriptions,
            OrdersByStatus: ordersByStatus,
            RecentOrders: recentOrders
        );
    }

    public async Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(int pageNumber, int pageSize, string? search, CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);

        var query = _db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Orders)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(u =>
                u.PhoneNumber.Contains(s) ||
                (u.Email != null && u.Email.Contains(s)) ||
                u.FullName.Contains(s));
        }

        query = query.OrderByDescending(u => u.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var dtos = items.Select(u => new AdminUserListItemDto(
            Id: u.Id,
            PublicId: u.PublicId,
            PhoneNumber: u.PhoneNumber,
            Email: u.Email,
            FullName: u.FullName,
            Status: u.Status,
            Roles: u.UserRoles.Select(ur => ur.Role?.Name ?? "").ToList(),
            CreatedAt: u.CreatedAt,
            TotalOrders: u.Orders.Count,
            TotalSpent: u.Orders.Where(o => o.OrderStatus != OrderStatuses.Cancelled).Sum(o => o.TotalAmount)
        )).ToList();

        return new PagedResult<AdminUserListItemDto>(dtos, total, page, size);
    }

    public async Task UpdateUserStatusAsync(long adminUserId, long targetUserId, UpdateUserStatusRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, ct)
            ?? throw new NotFoundException(nameof(User), targetUserId);

        var oldStatus = user.Status;
        user.Status = request.Status;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        _db.AdministrativeActions.Add(new AdministrativeAction
        {
            AdminUserId = adminUserId,
            ActionType = "user_status_update",
            Notes = $"Changed user {targetUserId} status from '{oldStatus}' to '{request.Status}'. Reason: {request.Reason}",
            TargetUserId = targetUserId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<WarehouseInventoryDto>> GetInventoryListAsync(int? warehouseId, bool lowStockOnly, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);

        var query = _db.Inventories
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(i => i.WarehouseId == warehouseId.Value);

        if (lowStockOnly)
            query = query.Where(i => (i.QuantityOnHand - i.QuantityReserved) <= i.ReorderLevel);

        query = query.OrderBy(i => i.QuantityOnHand - i.QuantityReserved);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var dtos = items.Select(i => new WarehouseInventoryDto(
            ProductId: i.ProductId,
            ProductName: i.Product?.Name ?? "Unknown Product",
            Sku: i.Product?.Sku ?? "",
            WarehouseId: i.WarehouseId,
            WarehouseName: i.Warehouse?.Name ?? "Main Warehouse",
            QuantityOnHand: i.QuantityOnHand,
            QuantityReserved: i.QuantityReserved,
            AvailableQuantity: i.QuantityOnHand - i.QuantityReserved,
            ReorderLevel: i.ReorderLevel,
            IsLowStock: (i.QuantityOnHand - i.QuantityReserved) <= i.ReorderLevel
        )).ToList();

        return new PagedResult<WarehouseInventoryDto>(dtos, total, page, size);
    }

    public async Task AdjustStockAsync(long adminUserId, StockAdjustmentRequest request, CancellationToken ct = default)
    {
        var inventory = await _db.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId, ct);

        if (inventory == null)
        {
            inventory = new Inventory
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                QuantityOnHand = 0,
                QuantityReserved = 0,
                ReorderLevel = 10,
                ReorderQuantity = 50,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.Inventories.Add(inventory);
        }

        var newOnHand = inventory.QuantityOnHand + request.QuantityChange;
        if (newOnHand < inventory.QuantityReserved)
        {
            throw new BusinessRuleViolationException(
                $"Cannot reduce stock below current reserved quantity ({inventory.QuantityReserved}). Requested on-hand would be {newOnHand}.");
        }

        inventory.QuantityOnHand = newOnHand;
        inventory.UpdatedAt = DateTimeOffset.UtcNow;

        _db.InventoryTransactions.Add(new InventoryTransaction
        {
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            TransactionType = "stock_adjustment",
            QuantityChange = request.QuantityChange,
            BalanceAfter = inventory.QuantityOnHand - inventory.QuantityReserved,
            ReferenceType = "manual_audit",
            Notes = request.Notes,
            CreatedBy = adminUserId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<ProductDetailDto> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var skuExists = await _db.Products.AnyAsync(p => p.Sku == request.Sku, ct);
        if (skuExists)
        {
            throw new ConflictException($"Product with SKU '{request.Sku}' already exists.");
        }

        var slugExists = await _db.Products.AnyAsync(p => p.Slug == request.Slug, ct);
        if (slugExists)
        {
            throw new ConflictException($"Product with Slug '{request.Slug}' already exists.");
        }

        var product = new Product
        {
            PublicId = Guid.NewGuid(),
            Sku = request.Sku.Trim(),
            Slug = request.Slug.Trim(),
            Name = request.Name.Trim(),
            NameBn = request.NameBn.Trim(),
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            ManufacturerId = request.ManufacturerId,
            GenericId = request.GenericId,
            DosageFormId = request.DosageFormId,
            Strength = request.Strength,
            PackSize = request.PackSize,
            UnitOfMeasure = request.UnitOfMeasure,
            UnitsPerPack = request.UnitsPerPack,
            BasePrice = request.BasePrice,
            DiscountPrice = request.DiscountPrice,
            IsPrescriptionRequired = request.IsPrescriptionRequired,
            IsOtc = request.IsOtc,
            Description = request.Description,
            DescriptionBn = request.DescriptionBn,
            UsageInstructions = request.UsageInstructions,
            StorageConditions = request.StorageConditions,
            IsFeatured = request.IsFeatured,
            Badge = request.Badge,
            IsActive = true,
            AvgRating = 0,
            ReviewCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        // Add initial inventory if specified
        if (request.InitialStock > 0)
        {
            _db.Inventories.Add(new Inventory
            {
                ProductId = product.Id,
                WarehouseId = request.InitialWarehouseId,
                QuantityOnHand = request.InitialStock,
                QuantityReserved = 0,
                ReorderLevel = 10,
                ReorderQuantity = 50,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = product.Id,
                WarehouseId = request.InitialWarehouseId,
                TransactionType = "purchase_receipt",
                QuantityChange = request.InitialStock,
                BalanceAfter = request.InitialStock,
                ReferenceType = "manual_audit",
                Notes = "Initial stock upon product creation",
                CreatedAt = DateTimeOffset.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }

        return await _catalogService.GetProductByIdAsync(product.Id, ct);
    }

    public async Task<ProductDetailDto> UpdateProductAsync(long productId, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.DeletedAt == null, ct)
            ?? throw new NotFoundException(nameof(Product), productId);

        product.Name = request.Name.Trim();
        product.NameBn = request.NameBn.Trim();
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.ManufacturerId = request.ManufacturerId;
        product.GenericId = request.GenericId;
        product.DosageFormId = request.DosageFormId;
        product.Strength = request.Strength;
        product.PackSize = request.PackSize;
        product.UnitOfMeasure = request.UnitOfMeasure;
        product.UnitsPerPack = request.UnitsPerPack;
        product.BasePrice = request.BasePrice;
        product.DiscountPrice = request.DiscountPrice;
        product.IsPrescriptionRequired = request.IsPrescriptionRequired;
        product.IsOtc = request.IsOtc;
        product.Description = request.Description;
        product.DescriptionBn = request.DescriptionBn;
        product.UsageInstructions = request.UsageInstructions;
        product.StorageConditions = request.StorageConditions;
        product.IsFeatured = request.IsFeatured;
        product.Badge = request.Badge;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return await _catalogService.GetProductByIdAsync(productId, ct);
    }

    public async Task DeleteProductAsync(long productId, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.DeletedAt == null, ct)
            ?? throw new NotFoundException(nameof(Product), productId);

        product.DeletedAt = DateTimeOffset.UtcNow;
        product.IsActive = false;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<CouponDto>> GetCouponsAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);

        var query = _db.Coupons.AsNoTracking().OrderByDescending(c => c.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var dtos = items.Select(MapToCouponDto).ToList();
        return new PagedResult<CouponDto>(dtos, total, page, size);
    }

    public async Task<CouponDto> CreateCouponAsync(CreateCouponRequest request, CancellationToken ct = default)
    {
        var exists = await _db.Coupons.AnyAsync(c => c.Code.ToUpper() == request.Code.ToUpper(), ct);
        if (exists)
        {
            throw new ConflictException($"Coupon with code '{request.Code}' already exists.");
        }

        var coupon = new Coupon
        {
            Code = request.Code.Trim().ToUpperInvariant(),
            Title = request.Title.Trim(),
            TitleBn = request.TitleBn?.Trim(),
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MinOrderAmount = request.MinOrderAmount,
            UsageLimitPerUser = request.UsageLimitPerUser,
            TotalUsageLimit = request.TotalUsageLimit,
            TimesUsed = 0,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            IsActive = request.IsActive,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync(ct);

        return MapToCouponDto(coupon);
    }

    public async Task<CouponDto> UpdateCouponAsync(int couponId, UpdateCouponRequest request, CancellationToken ct = default)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, ct)
            ?? throw new NotFoundException(nameof(Coupon), couponId);

        coupon.Title = request.Title.Trim();
        coupon.TitleBn = request.TitleBn?.Trim();
        coupon.Description = request.Description;
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.MaxDiscountAmount = request.MaxDiscountAmount;
        coupon.MinOrderAmount = request.MinOrderAmount;
        coupon.UsageLimitPerUser = request.UsageLimitPerUser;
        coupon.TotalUsageLimit = request.TotalUsageLimit;
        coupon.ValidFrom = request.ValidFrom;
        coupon.ValidUntil = request.ValidUntil;
        coupon.IsActive = request.IsActive;
        coupon.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return MapToCouponDto(coupon);
    }

    public async Task DeleteCouponAsync(int couponId, CancellationToken ct = default)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, ct)
            ?? throw new NotFoundException(nameof(Coupon), couponId);

        coupon.IsActive = false;
        coupon.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private static CouponDto MapToCouponDto(Coupon c) =>
        new(
            Id: c.Id,
            Code: c.Code,
            Title: c.Title,
            TitleBn: c.TitleBn,
            Description: c.Description,
            DiscountType: c.DiscountType,
            DiscountValue: c.DiscountValue,
            MaxDiscountAmount: c.MaxDiscountAmount,
            MinOrderAmount: c.MinOrderAmount,
            UsageLimitPerUser: c.UsageLimitPerUser,
            TotalUsageLimit: c.TotalUsageLimit,
            TimesUsed: c.TimesUsed,
            ValidFrom: c.ValidFrom,
            ValidUntil: c.ValidUntil,
            IsActive: c.IsActive
        );
}
