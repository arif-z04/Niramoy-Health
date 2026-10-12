using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;

namespace NiramoyHealth.Api.Controllers;

[Authorize]
public class WishlistController : BaseApiController
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist(CancellationToken ct)
    {
        var wishlist = await _wishlistService.GetWishlistAsync(RequiredUserId, ct);
        return OkResponse(wishlist);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToWishlistRequest request, CancellationToken ct)
    {
        var wishlist = await _wishlistService.AddItemAsync(RequiredUserId, request.ProductId, ct);
        return OkResponse(wishlist, "Product added to wishlist.");
    }

    [HttpDelete("items/{productId:long}")]
    public async Task<IActionResult> RemoveItem(long productId, CancellationToken ct)
    {
        var wishlist = await _wishlistService.RemoveItemAsync(RequiredUserId, productId, ct);
        return OkResponse(wishlist, "Product removed from wishlist.");
    }
}
