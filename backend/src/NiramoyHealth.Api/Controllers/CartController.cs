using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;

namespace NiramoyHealth.Api.Controllers;

public class CartController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken ct)
    {
        var cart = await _cartService.GetCartAsync(CurrentUserId, GuestTokenHeader, ct);
        return OkResponse(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request, CancellationToken ct)
    {
        var cart = await _cartService.AddItemAsync(CurrentUserId, GuestTokenHeader, request, ct);
        return OkResponse(cart, "Item added to cart.");
    }

    [HttpPut("items/{cartItemId:long}")]
    public async Task<IActionResult> UpdateQuantity(long cartItemId, [FromBody] UpdateCartItemRequest request, CancellationToken ct)
    {
        var cart = await _cartService.UpdateItemQuantityAsync(CurrentUserId, GuestTokenHeader, cartItemId, request.Quantity, ct);
        return OkResponse(cart, "Cart item updated.");
    }

    [HttpDelete("items/{cartItemId:long}")]
    public async Task<IActionResult> RemoveItem(long cartItemId, CancellationToken ct)
    {
        var cart = await _cartService.RemoveItemAsync(CurrentUserId, GuestTokenHeader, cartItemId, ct);
        return OkResponse(cart, "Item removed from cart.");
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        await _cartService.ClearCartAsync(CurrentUserId, GuestTokenHeader, ct);
        return OkResponse(true, "Cart cleared successfully.");
    }

    [HttpPost("merge")]
    public async Task<IActionResult> MergeGuestCart([FromQuery] Guid? guestToken, CancellationToken ct)
    {
        var token = (guestToken.HasValue && guestToken.Value != Guid.Empty) ? guestToken : GuestTokenHeader;
        if (!token.HasValue || token.Value == Guid.Empty)
        {
            var existingCart = await _cartService.GetCartAsync(RequiredUserId, null, ct);
            return OkResponse(existingCart, "No guest cart to merge.");
        }
        var cart = await _cartService.MergeGuestCartAsync(RequiredUserId, token.Value, ct);
        return OkResponse(cart, "Guest cart merged successfully.");
    }

    [HttpPost("coupon")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request, CancellationToken ct)
    {
        var cart = await _cartService.ApplyCouponAsync(CurrentUserId, GuestTokenHeader, request.CouponCode, ct);
        return OkResponse(cart, "Coupon applied successfully.");
    }

    [HttpDelete("coupon")]
    public async Task<IActionResult> RemoveCoupon(CancellationToken ct)
    {
        var cart = await _cartService.RemoveCouponAsync(CurrentUserId, GuestTokenHeader, ct);
        return OkResponse(cart, "Coupon removed.");
    }
}
