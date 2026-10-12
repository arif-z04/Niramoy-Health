using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(long? userId, Guid? guestToken, CancellationToken ct = default);
    Task<CartDto> AddItemAsync(long? userId, Guid? guestToken, AddToCartRequest request, CancellationToken ct = default);
    Task<CartDto> UpdateItemQuantityAsync(long? userId, Guid? guestToken, long cartItemId, int quantity, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(long? userId, Guid? guestToken, long cartItemId, CancellationToken ct = default);
    Task ClearCartAsync(long? userId, Guid? guestToken, CancellationToken ct = default);
    Task<CartDto> MergeGuestCartAsync(long userId, Guid guestToken, CancellationToken ct = default);
    Task<CartDto> ApplyCouponAsync(long? userId, Guid? guestToken, string couponCode, CancellationToken ct = default);
    Task<CartDto> RemoveCouponAsync(long? userId, Guid? guestToken, CancellationToken ct = default);
}
