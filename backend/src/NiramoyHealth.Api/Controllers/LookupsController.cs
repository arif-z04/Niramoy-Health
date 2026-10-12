using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.Interfaces;

namespace NiramoyHealth.Api.Controllers;

public class LookupsController : BaseApiController
{
    private readonly ICatalogService _catalogService;
    private readonly IOrderService _orderService;

    public LookupsController(ICatalogService catalogService, IOrderService orderService)
    {
        _catalogService = catalogService;
        _orderService = orderService;
    }

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(CancellationToken ct)
    {
        var brands = await _catalogService.GetBrandsAsync(ct);
        return OkResponse(brands);
    }

    [HttpGet("generics")]
    public async Task<IActionResult> GetGenerics(CancellationToken ct)
    {
        var generics = await _catalogService.GetGenericsAsync(ct);
        return OkResponse(generics);
    }

    [HttpGet("dosage-forms")]
    public async Task<IActionResult> GetDosageForms(CancellationToken ct)
    {
        var forms = await _catalogService.GetDosageFormsAsync(ct);
        return OkResponse(forms);
    }

    [HttpGet("delivery-methods")]
    public async Task<IActionResult> GetDeliveryMethods(CancellationToken ct)
    {
        var methods = await _orderService.GetDeliveryMethodsAsync(ct);
        return OkResponse(methods);
    }

    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethods(CancellationToken ct)
    {
        var methods = await _orderService.GetPaymentMethodsAsync(ct);
        return OkResponse(methods);
    }
}
