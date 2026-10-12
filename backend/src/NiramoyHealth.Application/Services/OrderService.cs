using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Enums;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class OrderService : IOrderService
{
    private readonly INiramoyDbContext _db;

    public OrderService(INiramoyDbContext db)
    {
        _db = db;
    }

    public async Task<OrderDto> CheckoutAsync(long userId, CheckoutRequest request, CancellationToken ct = default)
    {
        // Execute inside an explicit atomic database transaction
        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var user = await _db.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId, ct)
                ?? throw new NotFoundException(nameof(User), userId);

            // 1. Retrieve user's active cart
            var cart = await _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p!.Inventories)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "active", ct);

            if (cart == null || cart.Items.Count == 0)
            {
                throw new BusinessRuleViolationException("Your cart is empty. Cannot checkout an empty cart.");
            }

            // 2. Validate prescription requirements
            var containsPrescriptionItem = cart.Items.Any(i => i.Product != null && i.Product.IsPrescriptionRequired);
            if (containsPrescriptionItem)
            {
                if (!request.PrescriptionId.HasValue)
                {
                    throw new BusinessRuleViolationException("Your cart contains prescription-only medication. A verified prescription is mandatory to proceed with checkout.");
                }

                var prescription = await _db.Prescriptions
                    .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId.Value && p.UserId == userId, ct);

                if (prescription == null)
                {
                    throw new NotFoundException(nameof(Prescription), request.PrescriptionId.Value);
                }

                if (prescription.Status != PrescriptionStatuses.Approved && prescription.Status != PrescriptionStatuses.PartiallyApproved)
                {
                    throw new BusinessRuleViolationException($"The selected prescription is not approved (Current status: {prescription.Status}). Please wait for pharmacist verification.");
                }
            }

            // 3. Resolve shipping address
            OrderAddressInput addressInput;
            var targetAddressId = request.EffectiveAddressId;
            if (targetAddressId.HasValue)
            {
                var savedAddress = await _db.CustomerAddresses
                    .Include(a => a.Division)
                    .Include(a => a.District)
                    .Include(a => a.Upazila)
                    .FirstOrDefaultAsync(a => a.Id == targetAddressId.Value && a.UserId == userId && a.IsActive, ct);

                if (savedAddress == null)
                {
                    throw new NotFoundException(nameof(CustomerAddress), targetAddressId.Value);
                }

                addressInput = new OrderAddressInput(
                    RecipientName: savedAddress.RecipientName,
                    RecipientPhone: savedAddress.RecipientPhone,
                    Division: savedAddress.Division?.Name ?? "Dhaka",
                    District: savedAddress.District?.Name ?? "Dhaka",
                    Upazila: savedAddress.Upazila?.Name,
                    AreaDetails: savedAddress.AreaDetails,
                    PostalCode: savedAddress.PostalCode,
                    DeliveryInstructions: savedAddress.DeliveryInstructions
                );
            }
            else if (request.NewAddress != null)
            {
                addressInput = request.NewAddress;
            }
            else
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Address", new[] { "Shipping address is required." } }
                });
            }

            // 4. Resolve delivery method
            var deliveryMethod = await _db.DeliveryMethods
                .FirstOrDefaultAsync(d => d.Id == request.DeliveryMethodId && d.IsActive, ct);

            if (deliveryMethod == null)
            {
                throw new NotFoundException(nameof(DeliveryMethod), request.DeliveryMethodId);
            }

            // 5. Resolve payment method
            var paymentMethod = await _db.PaymentMethods
                .FirstOrDefaultAsync(p => p.Id == request.PaymentMethodId && p.IsActive, ct);

            if (paymentMethod == null)
            {
                throw new NotFoundException(nameof(PaymentMethod), request.PaymentMethodId);
            }

            // 6. Calculate pricing and lock item snapshots
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cart.Items)
            {
                var product = cartItem.Product;
                if (product == null || !product.IsActive || product.DeletedAt != null)
                {
                    throw new BusinessRuleViolationException($"Product (ID {cartItem.ProductId}) is no longer available.");
                }

                // Inventory check and reservation
                var inventory = await _db.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == product.Id, ct);

                var availableStock = inventory != null ? (inventory.QuantityOnHand - inventory.QuantityReserved) : 0;
                if (availableStock < cartItem.Quantity)
                {
                    throw new BusinessRuleViolationException(
                        $"Insufficient inventory for '{product.Name}'. Available: {Math.Max(0, availableStock)}, Requested: {cartItem.Quantity}.");
                }

                // Reserve inventory
                inventory!.QuantityReserved += cartItem.Quantity;
                inventory.UpdatedAt = DateTimeOffset.UtcNow;

                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    ProductId = product.Id,
                    WarehouseId = inventory.WarehouseId,
                    TransactionType = "reservation",
                    QuantityChange = cartItem.Quantity,
                    BalanceAfter = inventory.QuantityOnHand - inventory.QuantityReserved,
                    ReferenceType = "order",
                    ReferenceId = null, // Will update with order number
                    Notes = $"Reserved {cartItem.Quantity} for pending order",
                    CreatedBy = userId,
                    CreatedAt = DateTimeOffset.UtcNow
                });

                var unitPrice = product.DiscountPrice ?? product.BasePrice;
                var itemTotal = unitPrice * cartItem.Quantity;
                subtotal += itemTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductVariantId = null,
                    ProductName = product.Name,
                    ProductNameBn = product.NameBn,
                    ProductSku = product.Sku,
                    UnitPriceSnapshot = unitPrice,
                    Quantity = cartItem.Quantity,
                    TotalPrice = itemTotal,
                    IsPrescriptionItem = product.IsPrescriptionRequired,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            // Delivery fee calculation
            decimal deliveryFee = deliveryMethod.BaseFee;
            if (deliveryMethod.FreeDeliveryThreshold.HasValue && subtotal >= deliveryMethod.FreeDeliveryThreshold.Value)
            {
                deliveryFee = 0;
            }

            // Coupon calculation
            decimal discountAmount = 0;
            Coupon? appliedCoupon = null;
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var normalizedCode = request.CouponCode.Trim().ToUpperInvariant();
                appliedCoupon = await _db.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == normalizedCode && c.IsActive, ct);

                if (appliedCoupon != null &&
                    appliedCoupon.ValidFrom <= DateTimeOffset.UtcNow &&
                    appliedCoupon.ValidUntil >= DateTimeOffset.UtcNow &&
                    subtotal >= appliedCoupon.MinOrderAmount)
                {
                    var userUsageCount = await _db.CouponUsages
                        .CountAsync(u => u.CouponId == appliedCoupon.Id && u.UserId == userId, ct);

                    if (userUsageCount < appliedCoupon.UsageLimitPerUser)
                    {
                        if (appliedCoupon.DiscountType == DiscountTypes.Percentage)
                        {
                            discountAmount = (subtotal * appliedCoupon.DiscountValue) / 100m;
                            if (appliedCoupon.MaxDiscountAmount.HasValue && discountAmount > appliedCoupon.MaxDiscountAmount.Value)
                            {
                                discountAmount = appliedCoupon.MaxDiscountAmount.Value;
                            }
                        }
                        else if (appliedCoupon.DiscountType == DiscountTypes.FixedAmount)
                        {
                            discountAmount = Math.Min(subtotal, appliedCoupon.DiscountValue);
                        }
                    }
                }
            }

            decimal totalAmount = Math.Max(0, subtotal - discountAmount + deliveryFee);

            // 7. Generate order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";

            // 8. Create Order
            var order = new Order
            {
                PublicId = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = userId,
                PrescriptionId = containsPrescriptionItem ? request.PrescriptionId : null,
                OrderStatus = OrderStatuses.Pending,
                PaymentStatus = PaymentStatuses.Unpaid,
                SubtotalAmount = Math.Round(subtotal, 2),
                DiscountAmount = Math.Round(discountAmount, 2),
                DeliveryFee = Math.Round(deliveryFee, 2),
                TotalAmount = Math.Round(totalAmount, 2),
                Currency = "BDT",
                CouponCode = appliedCoupon?.Code,
                DeliveryMethodId = request.DeliveryMethodId,
                PaymentMethodId = request.PaymentMethodId,
                CustomerNotes = request.CustomerNotes,
                IdempotencyKey = request.IdempotencyKey,
                PlacedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            // 9. Attach child entities
            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                _db.OrderItems.Add(item);
            }

            var orderAddress = new OrderAddress
            {
                OrderId = order.Id,
                RecipientName = addressInput.RecipientName,
                RecipientPhone = addressInput.RecipientPhone,
                Division = addressInput.Division,
                District = addressInput.District,
                Upazila = addressInput.Upazila,
                AreaDetails = addressInput.AreaDetails,
                PostalCode = addressInput.PostalCode,
                DeliveryInstructions = addressInput.DeliveryInstructions,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.OrderAddresses.Add(orderAddress);

            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                PreviousStatus = null,
                NewStatus = OrderStatuses.Pending,
                ChangedBy = userId,
                Notes = "Order placed by customer",
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.OrderStatusHistories.Add(statusHistory);

            var payment = new Payment
            {
                PublicId = Guid.NewGuid(),
                OrderId = order.Id,
                PaymentMethodId = request.PaymentMethodId,
                PaymentStatus = PaymentTransactionStatuses.Pending,
                Amount = totalAmount,
                Currency = "BDT",
                GatewayProvider = paymentMethod.Code,
                InitiatedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.Payments.Add(payment);

            // Record coupon usage if applied
            if (appliedCoupon != null && discountAmount > 0)
            {
                _db.CouponUsages.Add(new CouponUsage
                {
                    CouponId = appliedCoupon.Id,
                    UserId = userId,
                    OrderId = order.Id,
                    DiscountApplied = discountAmount,
                    UsedAt = DateTimeOffset.UtcNow
                });
                appliedCoupon.TimesUsed++;
            }

            // 10. Clear cart items
            _db.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return await GetOrderByIdAsync(userId, order.Id, false, ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<PagedResult<OrderListSummaryDto>> GetUserOrdersAsync(long userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 50);

        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.PaymentMethod)
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.PlacedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var dtos = items.Select(MapToOrderListSummaryDto).ToList();
        return new PagedResult<OrderListSummaryDto>(dtos, total, page, size);
    }

    public async Task<OrderDto> GetOrderByIdAsync(long userId, long orderId, bool isStaff = false, CancellationToken ct = default)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.DeliveryMethod)
            .Include(o => o.PaymentMethod)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Images)
            .Include(o => o.StatusHistory)
            .Include(o => o.Payments)
            .ThenInclude(p => p.PaymentMethod)
            .Where(o => o.Id == orderId);

        if (!isStaff)
        {
            query = query.Where(o => o.UserId == userId);
        }

        var order = await query.FirstOrDefaultAsync(ct);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), orderId);
        }

        return MapToOrderDto(order);
    }

    public async Task<OrderDto> GetOrderByNumberAsync(long userId, string orderNumber, bool isStaff = false, CancellationToken ct = default)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.DeliveryMethod)
            .Include(o => o.PaymentMethod)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Images)
            .Include(o => o.StatusHistory)
            .Include(o => o.Payments)
            .ThenInclude(p => p.PaymentMethod)
            .Where(o => o.OrderNumber == orderNumber);

        if (!isStaff)
        {
            query = query.Where(o => o.UserId == userId);
        }

        var order = await query.FirstOrDefaultAsync(ct);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), orderNumber);
        }

        return MapToOrderDto(order);
    }

    public async Task<OrderDto> CancelOrderAsync(long userId, long orderId, CancelOrderRequest request, bool isStaff = false, CancellationToken ct = default)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var query = _db.Orders
                .Include(o => o.Items)
                .Where(o => o.Id == orderId);

            if (!isStaff)
            {
                query = query.Where(o => o.UserId == userId);
            }

            var order = await query.FirstOrDefaultAsync(ct);
            if (order == null)
            {
                throw new NotFoundException(nameof(Order), orderId);
            }

            if (order.OrderStatus != OrderStatuses.Pending && order.OrderStatus != OrderStatuses.Confirmed)
            {
                throw new BusinessRuleViolationException(
                    $"Order cannot be cancelled in its current state '{order.OrderStatus}'. Cancellation is only permitted before order processing begins.");
            }

            var prevStatus = order.OrderStatus;
            order.OrderStatus = OrderStatuses.Cancelled;
            order.CancelReason = request.Reason;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            // Release reserved inventory
            foreach (var item in order.Items)
            {
                var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId, ct);
                if (inventory != null)
                {
                    inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - item.Quantity);
                    inventory.UpdatedAt = DateTimeOffset.UtcNow;

                    _db.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = item.ProductId,
                        WarehouseId = inventory.WarehouseId,
                        TransactionType = "reservation_release",
                        QuantityChange = -item.Quantity,
                        BalanceAfter = inventory.QuantityOnHand - inventory.QuantityReserved,
                        ReferenceType = "order",
                        ReferenceId = order.OrderNumber,
                        Notes = $"Released {item.Quantity} units due to order cancellation",
                        CreatedBy = userId,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }

            _db.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                PreviousStatus = prevStatus,
                NewStatus = OrderStatuses.Cancelled,
                ChangedBy = userId,
                Notes = $"Order cancelled: {request.Reason}",
                CreatedAt = DateTimeOffset.UtcNow
            });

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return await GetOrderByIdAsync(userId, order.Id, isStaff, ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<DeliveryMethodDto>> GetDeliveryMethodsAsync(CancellationToken ct = default)
    {
        var methods = await _db.DeliveryMethods
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.BaseFee)
            .ToListAsync(ct);

        return methods.Select(m => new DeliveryMethodDto(
            m.Id, m.Code, m.Name, m.NameBn, m.BaseFee, m.FreeDeliveryThreshold, m.EstimatedDaysMin, m.EstimatedDaysMax
        )).ToList();
    }

    public async Task<IReadOnlyList<PaymentMethodDto>> GetPaymentMethodsAsync(CancellationToken ct = default)
    {
        var methods = await _db.PaymentMethods
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);

        return methods.Select(m => new PaymentMethodDto(
            m.Id, m.Code, m.Name, m.NameBn, m.IsMfs
        )).ToList();
    }

    public async Task<PagedResult<OrderListSummaryDto>> GetAllOrdersAsync(OrderFilterParams filter, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.PageNumber);
        var size = Math.Clamp(filter.PageSize, 1, 100);

        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.PaymentMethod)
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(o => o.OrderStatus == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
            query = query.Where(o => o.PaymentStatus == filter.PaymentStatus);

        if (filter.UserId.HasValue)
            query = query.Where(o => o.UserId == filter.UserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.OrderNumber))
            query = query.Where(o => o.OrderNumber.Contains(filter.OrderNumber));

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.PlacedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.PlacedAt <= filter.ToDate.Value);

        query = query.OrderByDescending(o => o.PlacedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var dtos = items.Select(MapToOrderListSummaryDto).ToList();
        return new PagedResult<OrderListSummaryDto>(dtos, total, page, size);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(long staffUserId, long orderId, UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);

            if (order == null)
            {
                throw new NotFoundException(nameof(Order), orderId);
            }

            var previousStatus = order.OrderStatus;
            var targetStatus = request.NewStatus.Trim().ToLowerInvariant();

            // Handle stock transition when status reaches "delivered"
            if (targetStatus == OrderStatuses.Delivered && previousStatus != OrderStatuses.Delivered)
            {
                foreach (var item in order.Items)
                {
                    var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == item.ProductId, ct);
                    if (inventory != null)
                    {
                        inventory.QuantityOnHand = Math.Max(0, inventory.QuantityOnHand - item.Quantity);
                        inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - item.Quantity);
                        inventory.UpdatedAt = DateTimeOffset.UtcNow;

                        _db.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = inventory.WarehouseId,
                            TransactionType = "sale_deduction",
                            QuantityChange = -item.Quantity,
                            BalanceAfter = inventory.QuantityOnHand - inventory.QuantityReserved,
                            ReferenceType = "order",
                            ReferenceId = order.OrderNumber,
                            Notes = $"Fulfilled delivery of {item.Quantity} units",
                            CreatedBy = staffUserId,
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                    }
                }
            }

            order.OrderStatus = targetStatus;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            _db.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                PreviousStatus = previousStatus,
                NewStatus = targetStatus,
                ChangedBy = staffUserId,
                Notes = request.Notes ?? $"Status updated to {targetStatus}",
                CreatedAt = DateTimeOffset.UtcNow
            });

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return await GetOrderByIdAsync(order.UserId, order.Id, true, ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private static OrderListSummaryDto MapToOrderListSummaryDto(Order o) =>
        new(
            Id: o.Id,
            PublicId: o.PublicId,
            OrderNumber: o.OrderNumber,
            UserId: o.UserId,
            CustomerName: o.User?.FullName ?? "Customer",
            CustomerPhone: o.User?.PhoneNumber ?? "",
            OrderStatus: o.OrderStatus,
            PaymentStatus: o.PaymentStatus,
            TotalAmount: o.TotalAmount,
            ItemCount: o.Items.Count,
            PaymentMethodName: o.PaymentMethod?.Name ?? "Cash",
            PlacedAt: o.PlacedAt
        );

    private static OrderDto MapToOrderDto(Order o)
    {
        var addressDto = o.DeliveryAddress != null
            ? new OrderAddressDto(
                RecipientName: o.DeliveryAddress.RecipientName,
                RecipientPhone: o.DeliveryAddress.RecipientPhone,
                Division: o.DeliveryAddress.Division,
                District: o.DeliveryAddress.District,
                Upazila: o.DeliveryAddress.Upazila,
                AreaDetails: o.DeliveryAddress.AreaDetails,
                PostalCode: o.DeliveryAddress.PostalCode,
                DeliveryInstructions: o.DeliveryAddress.DeliveryInstructions
            )
            : null;

        var itemDtos = o.Items.Select(i =>
        {
            var primaryImg = i.Product?.Images.FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                ?? i.Product?.Images.OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl;

            return new OrderItemDto(
                Id: i.Id,
                ProductId: i.ProductId,
                ProductVariantId: i.ProductVariantId,
                ProductName: i.ProductName,
                ProductNameBn: i.ProductNameBn,
                ProductSku: i.ProductSku,
                UnitPriceSnapshot: i.UnitPriceSnapshot,
                Quantity: i.Quantity,
                TotalPrice: i.TotalPrice,
                IsPrescriptionItem: i.IsPrescriptionItem,
                ProductImageUrl: primaryImg
            );
        }).ToList();

        var historyDtos = o.StatusHistory
            .OrderBy(h => h.CreatedAt)
            .Select(h => new OrderStatusHistoryDto(
                Id: h.Id,
                PreviousStatus: h.PreviousStatus,
                NewStatus: h.NewStatus,
                ChangedByName: h.ChangedBy.HasValue ? $"User #{h.ChangedBy.Value}" : "System",
                Notes: h.Notes,
                CreatedAt: h.CreatedAt
            )).ToList();

        var paymentDtos = o.Payments.Select(p => new PaymentSummaryDto(
            Id: p.Id,
            PublicId: p.PublicId,
            PaymentMethodCode: p.PaymentMethod?.Code ?? "unknown",
            PaymentMethodName: p.PaymentMethod?.Name ?? "Payment",
            PaymentStatus: p.PaymentStatus,
            Amount: p.Amount,
            Currency: p.Currency,
            TransactionId: p.TransactionId,
            GatewayProvider: p.GatewayProvider,
            InitiatedAt: p.InitiatedAt,
            CompletedAt: p.CompletedAt
        )).ToList();

        return new OrderDto(
            Id: o.Id,
            PublicId: o.PublicId,
            OrderNumber: o.OrderNumber,
            UserId: o.UserId,
            CustomerName: o.User?.FullName ?? "Customer",
            CustomerPhone: o.User?.PhoneNumber ?? "",
            PrescriptionId: o.PrescriptionId,
            OrderStatus: o.OrderStatus,
            PaymentStatus: o.PaymentStatus,
            SubtotalAmount: o.SubtotalAmount,
            DiscountAmount: o.DiscountAmount,
            DeliveryFee: o.DeliveryFee,
            TotalAmount: o.TotalAmount,
            Currency: o.Currency,
            CouponCode: o.CouponCode,
            DeliveryMethodId: o.DeliveryMethodId,
            DeliveryMethodName: o.DeliveryMethod?.Name ?? "Standard Delivery",
            PaymentMethodId: o.PaymentMethodId,
            PaymentMethodCode: o.PaymentMethod?.Code ?? "cod",
            PaymentMethodName: o.PaymentMethod?.Name ?? "Cash On Delivery",
            CustomerNotes: o.CustomerNotes,
            CancelReason: o.CancelReason,
            PlacedAt: o.PlacedAt,
            DeliveryAddress: addressDto,
            Items: itemDtos,
            StatusHistory: historyDtos,
            Payments: paymentDtos
        );
    }
}
