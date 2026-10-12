using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Enums;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class CartService : ICartService
{
    private readonly INiramoyDbContext _db;

    public CartService(INiramoyDbContext db)
    {
        _db = db;
    }

    public async Task<CartDto> GetCartAsync(long? userId, Guid? guestToken, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, guestToken, ct);
        return await CalculateCartDtoAsync(cart, null, ct);
    }

    public async Task<CartDto> AddItemAsync(long? userId, Guid? guestToken, AddToCartRequest request, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, guestToken, ct);

        var product = await _db.Products
            .Include(p => p.Inventories)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive && p.DeletedAt == null, ct);

        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        var availableStock = product.Inventories.Sum(i => i.QuantityOnHand - i.QuantityReserved);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        var targetQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (targetQuantity > availableStock)
        {
            throw new BusinessRuleViolationException(
                $"Requested quantity ({targetQuantity}) exceeds available stock ({Math.Max(0, availableStock)}) for product '{product.Name}'.");
        }

        if (existingItem != null)
        {
            existingItem.Quantity = targetQuantity;
            existingItem.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                AddedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            cart.Items.Add(newItem);
        }

        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await CalculateCartDtoAsync(cart, null, ct);
    }

    public async Task<CartDto> UpdateItemQuantityAsync(long? userId, Guid? guestToken, long cartItemId, int quantity, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, guestToken, ct);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null)
        {
            throw new NotFoundException(nameof(CartItem), cartItemId);
        }

        if (quantity <= 0)
        {
            _db.CartItems.Remove(item);
            cart.Items.Remove(item);
        }
        else
        {
            var product = await _db.Products
                .Include(p => p.Inventories)
                .FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);

            if (product != null)
            {
                var availableStock = product.Inventories.Sum(i => i.QuantityOnHand - i.QuantityReserved);
                if (quantity > availableStock)
                {
                    throw new BusinessRuleViolationException(
                        $"Requested quantity ({quantity}) exceeds available stock ({Math.Max(0, availableStock)}) for product '{product.Name}'.");
                }
            }

            item.Quantity = quantity;
            item.UpdatedAt = DateTimeOffset.UtcNow;
        }

        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await CalculateCartDtoAsync(cart, null, ct);
    }

    public async Task<CartDto> RemoveItemAsync(long? userId, Guid? guestToken, long cartItemId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, guestToken, ct);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item != null)
        {
            _db.CartItems.Remove(item);
            cart.Items.Remove(item);
            cart.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return await CalculateCartDtoAsync(cart, null, ct);
    }

    public async Task ClearCartAsync(long? userId, Guid? guestToken, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, guestToken, ct);
        if (cart.Items.Count > 0)
        {
            _db.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<CartDto> MergeGuestCartAsync(long userId, Guid guestToken, CancellationToken ct = default)
    {
        var guestCart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.GuestToken == guestToken && c.Status == "active", ct);

        var userCart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "active", ct);

        if (userCart == null)
        {
            userCart = new Cart
            {
                PublicId = Guid.NewGuid(),
                UserId = userId,
                Status = "active",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.Carts.Add(userCart);
            await _db.SaveChangesAsync(ct);
        }

        if (guestCart != null && guestCart.Items.Count > 0)
        {
            foreach (var guestItem in guestCart.Items)
            {
                var userItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
                if (userItem != null)
                {
                    userItem.Quantity += guestItem.Quantity;
                    userItem.UpdatedAt = DateTimeOffset.UtcNow;
                }
                else
                {
                    userCart.Items.Add(new CartItem
                    {
                        CartId = userCart.Id,
                        ProductId = guestItem.ProductId,
                        Quantity = guestItem.Quantity,
                        AddedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                }
            }

            guestCart.Items.Clear();
            guestCart.Status = CartStatuses.Converted;
            guestCart.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return await CalculateCartDtoAsync(userCart, null, ct);
    }

    public async Task<CartDto> ApplyCouponAsync(long? userId, Guid? guestToken, string couponCode, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, guestToken, ct);
        var normalizedCode = couponCode.Trim().ToUpperInvariant();

        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == normalizedCode && c.IsActive, ct);

        if (coupon == null || coupon.ValidFrom > DateTimeOffset.UtcNow || coupon.ValidUntil < DateTimeOffset.UtcNow)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "CouponCode", new[] { "Coupon code is invalid or expired." } }
            });
        }

        return await CalculateCartDtoAsync(cart, coupon, ct);
    }

    public async Task<CartDto> RemoveCouponAsync(long? userId, Guid? guestToken, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, guestToken, ct);
        return await CalculateCartDtoAsync(cart, null, ct);
    }

    private async Task<Cart> GetOrCreateCartAsync(long? userId, Guid? guestToken, CancellationToken ct)
    {
        Cart? cart = null;

        if (userId.HasValue)
        {
            cart = await _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p!.Images)
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p!.Inventories)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.Status == "active", ct);

            if (cart == null)
            {
                cart = new Cart
                {
                    PublicId = Guid.NewGuid(),
                    UserId = userId.Value,
                    Status = "active",
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync(ct);
            }
        }
        else if (guestToken.HasValue)
        {
            cart = await _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p!.Images)
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p!.Inventories)
                .FirstOrDefaultAsync(c => c.GuestToken == guestToken.Value && c.Status == "active", ct);

            if (cart == null)
            {
                cart = new Cart
                {
                    PublicId = Guid.NewGuid(),
                    GuestToken = guestToken.Value,
                    Status = "active",
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync(ct);
            }
        }
        else
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Cart", new[] { "Either user session or guest token is required." } }
            });
        }

        return cart;
    }

    private async Task<CartDto> CalculateCartDtoAsync(Cart cart, Coupon? coupon, CancellationToken ct)
    {
        var itemDtos = new List<CartItemDto>();
        decimal subtotal = 0;
        bool requiresPrescription = false;
        int totalQuantity = 0;

        // Ensure all product navigations are loaded
        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Inventories)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        foreach (var item in cart.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive || product.DeletedAt != null)
                continue;

            var availableStock = product.Inventories.Sum(i => i.QuantityOnHand - i.QuantityReserved);
            var currentPrice = product.DiscountPrice ?? product.BasePrice;
            var itemTotal = currentPrice * item.Quantity;

            subtotal += itemTotal;
            totalQuantity += item.Quantity;
            if (product.IsPrescriptionRequired)
            {
                requiresPrescription = true;
            }

            var primaryImg = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ?? product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;

            itemDtos.Add(new CartItemDto(
                Id: item.Id,
                ProductId: product.Id,
                Sku: product.Sku,
                Slug: product.Slug,
                Name: product.Name,
                NameBn: product.NameBn,
                Strength: product.Strength,
                PackSize: product.PackSize,
                PrimaryImageUrl: primaryImg,
                BasePrice: product.BasePrice,
                DiscountPrice: product.DiscountPrice,
                CurrentPrice: currentPrice,
                Quantity: item.Quantity,
                ItemTotal: itemTotal,
                IsPrescriptionRequired: product.IsPrescriptionRequired,
                IsInStock: availableStock >= item.Quantity,
                AvailableStock: availableStock
            ));
        }

        decimal discountAmount = 0;
        if (coupon != null && subtotal >= coupon.MinOrderAmount)
        {
            if (coupon.DiscountType == DiscountTypes.Percentage)
            {
                discountAmount = (subtotal * coupon.DiscountValue) / 100m;
                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = coupon.MaxDiscountAmount.Value;
                }
            }
            else if (coupon.DiscountType == DiscountTypes.FixedAmount)
            {
                discountAmount = Math.Min(subtotal, coupon.DiscountValue);
            }
        }

        // Standard delivery fee: 60 BDT inside Dhaka; free over 1000 BDT
        decimal deliveryFee = subtotal >= 1000m || subtotal == 0 ? 0m : 60m;
        decimal grandTotal = Math.Max(0m, subtotal - discountAmount + deliveryFee);

        return new CartDto(
            Id: cart.Id,
            PublicId: cart.PublicId,
            UserId: cart.UserId,
            GuestToken: cart.GuestToken,
            Items: itemDtos,
            TotalItemCount: itemDtos.Count,
            TotalItemQuantity: totalQuantity,
            Subtotal: Math.Round(subtotal, 2),
            DeliveryFee: Math.Round(deliveryFee, 2),
            DiscountAmount: Math.Round(discountAmount, 2),
            GrandTotal: Math.Round(grandTotal, 2),
            RequiresPrescription: requiresPrescription,
            AppliedCouponCode: coupon?.Code
        );
    }
}
