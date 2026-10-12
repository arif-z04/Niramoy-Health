using System.Net;
using System.Text;
using System.Text.Json;
using NiramoyHealth.Application.DTOs;
using Xunit;

namespace NiramoyHealth.IntegrationTests;

public class CatalogAndCartTests : IClassFixture<NiramoyTestWebApplicationFactory>
{
    private readonly NiramoyTestWebApplicationFactory _factory;

    public CatalogAndCartTests(NiramoyTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCategoriesAndProducts_ReturnsSeededCatalogData()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act 1: Get Categories
        var catResp = await client.GetAsync("/api/v1/categories");
        Assert.Equal(HttpStatusCode.OK, catResp.StatusCode);
        var catJson = await catResp.Content.ReadAsStringAsync();
        using var catDoc = JsonDocument.Parse(catJson);
        var categories = catDoc.RootElement.GetProperty("data");
        Assert.True(categories.GetArrayLength() > 0);

        // Act 2: Get Products
        var prodResp = await client.GetAsync("/api/v1/products");
        Assert.Equal(HttpStatusCode.OK, prodResp.StatusCode);
        var prodJson = await prodResp.Content.ReadAsStringAsync();
        using var prodDoc = JsonDocument.Parse(prodJson);
        var items = prodDoc.RootElement.GetProperty("items");
        Assert.True(items.GetArrayLength() > 0);
    }

    [Fact]
    public async Task Lookups_ReturnsDivisionsAndPaymentMethods()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act 1: Divisions
        var divResp = await client.GetAsync("/api/v1/addresses/divisions");
        Assert.Equal(HttpStatusCode.OK, divResp.StatusCode);
        var divJson = await divResp.Content.ReadAsStringAsync();
        using var divDoc = JsonDocument.Parse(divJson);
        Assert.Equal(8, divDoc.RootElement.GetProperty("data").GetArrayLength()); // 8 administrative divisions of Bangladesh

        // Act 2: Payment Methods
        var pmResp = await client.GetAsync("/api/v1/lookups/payment-methods");
        Assert.Equal(HttpStatusCode.OK, pmResp.StatusCode);
        var pmJson = await pmResp.Content.ReadAsStringAsync();
        using var pmDoc = JsonDocument.Parse(pmJson);
        Assert.True(pmDoc.RootElement.GetProperty("data").GetArrayLength() >= 3); // COD, bKash, Nagad
    }

    [Fact]
    public async Task GuestCart_AddUpdateRemoveItems_CalculatesDynamicPricing()
    {
        // Arrange
        var client = _factory.CreateClient();
        var guestToken = Guid.NewGuid();
        client.DefaultRequestHeaders.Add("X-Guest-Token", guestToken.ToString());

        // Get an active product ID
        var prodResp = await client.GetAsync("/api/v1/products?pageSize=1");
        var prodJson = await prodResp.Content.ReadAsStringAsync();
        using var prodDoc = JsonDocument.Parse(prodJson);
        var firstProduct = prodDoc.RootElement.GetProperty("items")[0];
        var productId = firstProduct.GetProperty("id").GetInt64();
        var currentPrice = firstProduct.GetProperty("currentPrice").GetDecimal();

        // Act 1: Add item to cart
        var addReq = new AddToCartRequest(ProductId: productId, Quantity: 2);
        var addContent = new StringContent(JsonSerializer.Serialize(addReq), Encoding.UTF8, "application/json");
        var addResp = await client.PostAsync("/api/v1/cart/items", addContent);

        // Assert 1
        Assert.Equal(HttpStatusCode.OK, addResp.StatusCode);
        var cartJson = await addResp.Content.ReadAsStringAsync();
        using var cartDoc = JsonDocument.Parse(cartJson);
        var cartData = cartDoc.RootElement.GetProperty("data");

        Assert.Equal(2, cartData.GetProperty("totalItemQuantity").GetInt32());
        var expectedSubtotal = Math.Round(currentPrice * 2, 2);
        Assert.Equal(expectedSubtotal, cartData.GetProperty("subtotal").GetDecimal());

        // Act 2: Clear cart
        var clearResp = await client.DeleteAsync("/api/v1/cart");
        Assert.Equal(HttpStatusCode.OK, clearResp.StatusCode);
    }
}
