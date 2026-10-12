using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.Interfaces;

namespace NiramoyHealth.Api.Controllers;

public class CategoriesController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public CategoriesController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await _catalogService.GetCategoriesAsync(ct);
        return OkResponse(categories);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(string slug, CancellationToken ct)
    {
        var category = await _catalogService.GetCategoryBySlugAsync(slug, ct);
        return OkResponse(category);
    }
}
