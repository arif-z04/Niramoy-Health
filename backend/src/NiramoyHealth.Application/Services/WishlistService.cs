using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly INiramoyDbContext _db;

    public WishlistService(INiramoyDbContext db)
    {
        _db = db;
    }

    public async Task<WishlistDto> GetWishlistAsync(long userId, CancellationToken ct = default)
    {
        var wishlist = await GetOrCreateWishlistAsync(userId, ct);
        return MapToWishlistDto(wishlist);
    }

    public async Task<WishlistDto> AddItemAsync(long userId, long productId, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive && p.DeletedAt == null, ct)
            ?? throw new NotFoundException(nameof(Product), productId);

        var wishlist = await GetOrCreateWishlistAsync(userId, ct);

        if (!wishlist.Items.Any(i => i.ProductId == productId))
        {
            var item = new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = productId,
                AddedAt = DateTimeOffset.UtcNow
            };
            wishlist.Items.Add(item);
            wishlist.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return await GetWishlistAsync(userId, ct);
    }

    public async Task<WishlistDto> RemoveItemAsync(long userId, long productId, CancellationToken ct = default)
    {
        var wishlist = await GetOrCreateWishlistAsync(userId, ct);
        var item = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item != null)
        {
            _db.WishlistItems.Remove(item);
            wishlist.Items.Remove(item);
            wishlist.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return await GetWishlistAsync(userId, ct);
    }

    private async Task<Wishlist> GetOrCreateWishlistAsync(long userId, CancellationToken ct)
    {
        var wishlist = await _db.Wishlists
            .Include(w => w.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Images)
            .Include(w => w.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Inventories)
            .FirstOrDefaultAsync(w => w.UserId == userId, ct);

        if (wishlist == null)
        {
            wishlist = new Wishlist
            {
                UserId = userId,
                Name = "My Wishlist",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.Wishlists.Add(wishlist);
            await _db.SaveChangesAsync(ct);
        }

        return wishlist;
    }

    private static WishlistDto MapToWishlistDto(Wishlist w)
    {
        var items = w.Items
            .Where(i => i.Product != null && i.Product.IsActive && i.Product.DeletedAt == null)
            .Select(i =>
            {
                var p = i.Product!;
                var stock = p.Inventories.Sum(inv => inv.QuantityOnHand - inv.QuantityReserved);
                var primaryImg = p.Images.FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                    ?? p.Images.OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl;

                return new WishlistItemDto(
                    Id: i.Id,
                    ProductId: p.Id,
                    Sku: p.Sku,
                    Slug: p.Slug,
                    Name: p.Name,
                    NameBn: p.NameBn,
                    Strength: p.Strength,
                    BasePrice: p.BasePrice,
                    DiscountPrice: p.DiscountPrice,
                    CurrentPrice: p.DiscountPrice ?? p.BasePrice,
                    PrimaryImageUrl: primaryImg,
                    IsInStock: stock > 0,
                    AddedAt: i.AddedAt
                );
            })
            .ToList();

        return new WishlistDto(w.Id, w.UserId, w.Name, items);
    }
}
