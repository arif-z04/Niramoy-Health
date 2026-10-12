using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public interface IWishlistService
{
    Task<WishlistDto> GetWishlistAsync(long userId, CancellationToken ct = default);
    Task<WishlistDto> AddItemAsync(long userId, long productId, CancellationToken ct = default);
    Task<WishlistDto> RemoveItemAsync(long userId, long productId, CancellationToken ct = default);
}
