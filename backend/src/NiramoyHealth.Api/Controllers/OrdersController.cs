using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;

namespace NiramoyHealth.Api.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var order = await _orderService.CheckoutAsync(RequiredUserId, request, ct);
        return OkResponse(order, "Order placed successfully.");
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var activePage = page > 1 ? page : pageNumber;
        var orders = await _orderService.GetUserOrdersAsync(RequiredUserId, activePage, pageSize, ct);
        return OkResponse(orders);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetOrderById(long id, CancellationToken ct)
    {
        var order = await _orderService.GetOrderByIdAsync(RequiredUserId, id, IsStaffUser, ct);
        return OkResponse(order);
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<IActionResult> GetOrderByNumber(string orderNumber, CancellationToken ct)
    {
        var order = await _orderService.GetOrderByNumberAsync(RequiredUserId, orderNumber, IsStaffUser, ct);
        return OkResponse(order);
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> CancelOrder(long id, [FromBody] CancelOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.CancelOrderAsync(RequiredUserId, id, request, IsStaffUser, ct);
        return OkResponse(order, "Order cancelled successfully.");
    }
}
