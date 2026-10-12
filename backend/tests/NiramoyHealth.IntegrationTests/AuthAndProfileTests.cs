using System.Net;
using System.Text;
using System.Text.Json;
using NiramoyHealth.Application.DTOs;
using Xunit;

namespace NiramoyHealth.IntegrationTests;

public class AuthAndProfileTests : IClassFixture<NiramoyTestWebApplicationFactory>
{
    private readonly NiramoyTestWebApplicationFactory _factory;

    public AuthAndProfileTests(NiramoyTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterAndLogin_Succeeds_WithValidBangladeshiPhoneNumber()
    {
        // Arrange
        var client = _factory.CreateClient();
        var uniquePhone = $"0181{Random.Shared.Next(100000, 999999)}";
        var password = "SecurePassword2026!";

        var registerReq = new RegisterRequest(
            PhoneNumber: uniquePhone,
            FullName: "Kazi Nazrul",
            Email: $"kazi{Random.Shared.Next(1000, 9999)}@example.com",
            Password: password
        );

        // Act 1: Register
        var regContent = new StringContent(JsonSerializer.Serialize(registerReq), Encoding.UTF8, "application/json");
        var regResponse = await client.PostAsync("/api/v1/auth/register", regContent);

        // Assert 1
        Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);
        var regJson = await regResponse.Content.ReadAsStringAsync();
        using var regDoc = JsonDocument.Parse(regJson);
        Assert.True(regDoc.RootElement.GetProperty("success").GetBoolean());
        var token = regDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        // Act 2: Login
        var loginReq = new LoginRequest(uniquePhone, password);
        var loginContent = new StringContent(JsonSerializer.Serialize(loginReq), Encoding.UTF8, "application/json");
        var loginResponse = await client.PostAsync("/api/v1/auth/login", loginContent);

        // Assert 2
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginJson = await loginResponse.Content.ReadAsStringAsync();
        using var loginDoc = JsonDocument.Parse(loginJson);
        Assert.True(loginDoc.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Login_Fails_WithIncorrectPassword()
    {
        // Arrange
        var client = _factory.CreateClient();
        var uniquePhone = $"0172{Random.Shared.Next(100000, 999999)}";
        var password = "CorrectPassword123";

        // Register first
        var regReq = new RegisterRequest(uniquePhone, "Test User", null, password);
        var regContent = new StringContent(JsonSerializer.Serialize(regReq), Encoding.UTF8, "application/json");
        await client.PostAsync("/api/v1/auth/register", regContent);

        // Act: Login with wrong password
        var loginReq = new LoginRequest(uniquePhone, "WrongPassword999");
        var loginContent = new StringContent(JsonSerializer.Serialize(loginReq), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/auth/login", loginContent);

        // Assert: 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfileAndAddresses_Succeeds_ForAuthenticatedUser()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();

        // Act: Get Profile
        var profileResp = await client.GetAsync("/api/v1/auth/profile");

        // Assert
        Assert.Equal(HttpStatusCode.OK, profileResp.StatusCode);
        var profileJson = await profileResp.Content.ReadAsStringAsync();
        using var profileDoc = JsonDocument.Parse(profileJson);
        Assert.True(profileDoc.RootElement.GetProperty("success").GetBoolean());

        // Act: Get Addresses (should be empty for new user)
        var addrResp = await client.GetAsync("/api/v1/addresses");
        Assert.Equal(HttpStatusCode.OK, addrResp.StatusCode);
    }
}
