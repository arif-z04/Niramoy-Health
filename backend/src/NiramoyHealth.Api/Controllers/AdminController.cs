using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Enums;

namespace NiramoyHealth.Api.Controllers;

[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.SuperAdmin}")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;

    public AdminController(IAdminService adminService, IOrderService orderService, IPaymentService paymentService)
    {
        _adminService = adminService;
        _orderService = orderService;
        _paymentService = paymentService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardMetrics(CancellationToken ct)
    {
        var metrics = await _adminService.GetDashboardMetricsAsync(ct);
        return OkResponse(metrics);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders([FromQuery] OrderFilterParams filter, CancellationToken ct)
    {
        var orders = await _orderService.GetAllOrdersAsync(filter, ct);
        return Ok(orders);
    }

    [HttpPut("orders/{id:long}/status")]
    public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var order = await _orderService.UpdateOrderStatusAsync(RequiredUserId, id, request, ct);
        return OkResponse(order, $"Order status updated to {order.OrderStatus}.");
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryList(
        [FromQuery] int? warehouseId,
        [FromQuery] bool lowStockOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var inventory = await _adminService.GetInventoryListAsync(warehouseId, lowStockOnly, pageNumber, pageSize, ct);
        return Ok(inventory);
    }

    [HttpPost("inventory/adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentRequest request, CancellationToken ct)
    {
        await _adminService.AdjustStockAsync(RequiredUserId, request, ct);
        return OkResponse(true, "Inventory adjusted successfully.");
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var users = await _adminService.GetUsersAsync(pageNumber, pageSize, search, ct);
        return Ok(users);
    }

    [HttpPut("users/{id:long}/status")]
    public async Task<IActionResult> UpdateUserStatus(long id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        await _adminService.UpdateUserStatusAsync(RequiredUserId, id, request, ct);
        return OkResponse(true, $"User status updated to {request.Status}.");
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await _adminService.CreateProductAsync(request, ct);
        return OkResponse(product, "Product created successfully.");
    }

    [HttpPut("products/{id:long}")]
    public async Task<IActionResult> UpdateProduct(long id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var product = await _adminService.UpdateProductAsync(id, request, ct);
        return OkResponse(product, "Product updated successfully.");
    }

    [HttpDelete("products/{id:long}")]
    public async Task<IActionResult> DeleteProduct(long id, CancellationToken ct)
    {
        await _adminService.DeleteProductAsync(id, ct);
        return OkResponse(true, "Product deleted successfully.");
    }

    [HttpGet("coupons")]
    public async Task<IActionResult> GetCoupons(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var coupons = await _adminService.GetCouponsAsync(pageNumber, pageSize, ct);
        return Ok(coupons);
    }

    [HttpPost("coupons")]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request, CancellationToken ct)
    {
        var coupon = await _adminService.CreateCouponAsync(request, ct);
        return OkResponse(coupon, "Coupon created successfully.");
    }

    [HttpPut("coupons/{id:int}")]
    public async Task<IActionResult> UpdateCoupon(int id, [FromBody] UpdateCouponRequest request, CancellationToken ct)
    {
        var coupon = await _adminService.UpdateCouponAsync(id, request, ct);
        return OkResponse(coupon, "Coupon updated successfully.");
    }

    [HttpDelete("coupons/{id:int}")]
    public async Task<IActionResult> DeleteCoupon(int id, CancellationToken ct)
    {
        await _adminService.DeleteCouponAsync(id, ct);
        return OkResponse(true, "Coupon deleted successfully.");
    }

    [HttpPost("refunds")]
    public async Task<IActionResult> ProcessRefund([FromBody] ProcessRefundRequest request, CancellationToken ct)
    {
        var refund = await _paymentService.ProcessRefundAsync(RequiredUserId, request, ct);
        return OkResponse(refund, "Refund processed successfully.");
    }
}
