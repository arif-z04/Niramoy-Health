using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;

namespace NiramoyHealth.Api.Controllers;

public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize]
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request, CancellationToken ct)
    {
        var result = await _paymentService.InitiatePaymentAsync(RequiredUserId, request.OrderId, ct);
        return OkResponse(result, "Payment initialized successfully.");
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecutePayment([FromBody] ExecutePaymentRequest request, CancellationToken ct)
    {
        var result = await _paymentService.ExecutePaymentAsync(request, ct);
        return OkResponse(result, result.Message);
    }

    [HttpPost("webhook/{provider}")]
    public async Task<IActionResult> PaymentWebhook(
        string provider,
        [FromQuery] Guid? paymentId,
        [FromQuery] string? trxId,
        CancellationToken ct)
    {
        if (paymentId.HasValue)
        {
            var req = new ExecutePaymentRequest(paymentId.Value, null, trxId);
            var result = await _paymentService.ExecutePaymentAsync(req, ct);
            return Ok(result);
        }

        return Ok(new { received = true, provider, timestamp = DateTime.UtcNow });
    }
}
