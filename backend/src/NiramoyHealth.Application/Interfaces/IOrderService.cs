using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CheckoutAsync(long userId, CheckoutRequest request, CancellationToken ct = default);
    Task<PagedResult<OrderListSummaryDto>> GetUserOrdersAsync(long userId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<OrderDto> GetOrderByIdAsync(long userId, long orderId, bool isStaff = false, CancellationToken ct = default);
    Task<OrderDto> GetOrderByNumberAsync(long userId, string orderNumber, bool isStaff = false, CancellationToken ct = default);
    Task<OrderDto> CancelOrderAsync(long userId, long orderId, CancelOrderRequest request, bool isStaff = false, CancellationToken ct = default);
    Task<IReadOnlyList<DeliveryMethodDto>> GetDeliveryMethodsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PaymentMethodDto>> GetPaymentMethodsAsync(CancellationToken ct = default);
    Task<PagedResult<OrderListSummaryDto>> GetAllOrdersAsync(OrderFilterParams filter, CancellationToken ct = default);
    Task<OrderDto> UpdateOrderStatusAsync(long staffUserId, long orderId, UpdateOrderStatusRequest request, CancellationToken ct = default);
}
