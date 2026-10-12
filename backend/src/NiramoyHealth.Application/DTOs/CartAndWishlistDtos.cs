namespace NiramoyHealth.Application.DTOs;

public record CartItemDto(
    long Id,
    long ProductId,
    string Sku,
    string Slug,
    string Name,
    string NameBn,
    string? Strength,
    string? PackSize,
    string? PrimaryImageUrl,
    decimal BasePrice,
    decimal? DiscountPrice,
    decimal CurrentPrice,
    int Quantity,
    decimal ItemTotal,
    bool IsPrescriptionRequired,
    bool IsInStock,
    int AvailableStock
);

public record CartDto(
    long Id,
    Guid PublicId,
    long? UserId,
    Guid? GuestToken,
    IReadOnlyList<CartItemDto> Items,
    int TotalItemCount,
    int TotalItemQuantity,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal DiscountAmount,
    decimal GrandTotal,
    bool RequiresPrescription,
    string? AppliedCouponCode
);

public record AddToCartRequest(
    long ProductId,
    int Quantity = 1
);

public record UpdateCartItemRequest(
    int Quantity
);

public record ApplyCouponRequest(
    string CouponCode
);

public record WishlistItemDto(
    long Id,
    long ProductId,
    string Sku,
    string Slug,
    string Name,
    string NameBn,
    string? Strength,
    decimal BasePrice,
    decimal? DiscountPrice,
    decimal CurrentPrice,
    string? PrimaryImageUrl,
    bool IsInStock,
    DateTimeOffset AddedAt
);

public record WishlistDto(
    long Id,
    long UserId,
    string Name,
    IReadOnlyList<WishlistItemDto> Items
);

public record AddToWishlistRequest(
    long ProductId
);
