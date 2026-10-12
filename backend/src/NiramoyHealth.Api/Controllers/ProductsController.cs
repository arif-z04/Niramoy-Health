using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;

namespace NiramoyHealth.Api.Controllers;

public class ProductsController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public ProductsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterParams filter, CancellationToken ct)
    {
        var result = await _catalogService.GetProductsAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedProducts([FromQuery] int count = 10, CancellationToken ct = default)
    {
        var products = await _catalogService.GetFeaturedProductsAsync(count, ct);
        return OkResponse(products);
    }

    [HttpGet("deals")]
    [HttpGet("discounted")]
    public async Task<IActionResult> GetDiscountDeals([FromQuery] int count = 10, CancellationToken ct = default)
    {
        var products = await _catalogService.GetDiscountDealsAsync(count, ct);
        return OkResponse(products);
    }

    [HttpGet("id/{id:long}")]
    public async Task<IActionResult> GetProductById(long id, CancellationToken ct)
    {
        var product = await _catalogService.GetProductByIdAsync(id, ct);
        return OkResponse(product);
    }

    [HttpGet("{slug}")]
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetProductBySlug(string slug, CancellationToken ct)
    {
        var product = await _catalogService.GetProductBySlugAsync(slug, ct);
        return OkResponse(product);
    }

    [HttpGet("{id:long}/reviews")]
    public async Task<IActionResult> GetProductReviews(long id, CancellationToken ct)
    {
        var reviews = await _catalogService.GetProductReviewsAsync(id, ct);
        return OkResponse(reviews);
    }

    [Authorize]
    [HttpPost("{id:long}/reviews")]
    public async Task<IActionResult> AddProductReview(long id, [FromBody] CreateProductReviewRequest request, CancellationToken ct)
    {
        var review = await _catalogService.AddProductReviewAsync(RequiredUserId, id, request, ct);
        return OkResponse(review, "Review submitted successfully.");
    }
}
