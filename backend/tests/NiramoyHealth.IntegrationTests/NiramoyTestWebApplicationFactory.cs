using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Domain.Enums;
using NiramoyHealth.Infrastructure.Persistence;

namespace NiramoyHealth.IntegrationTests;

public class NiramoyTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
        "Host=127.0.0.1;Port=5432;Database=niramoy_health_test;Username=niramoy_user;Password=niramoy_dev_pass_2026;Pooling=true;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["Jwt:SecretKey"] = "NiramoyHealth_SuperSecret_Jwt_EncryptionKey_2026_Bangladesh_Security!",
                ["Jwt:Issuer"] = "NiramoyHealth",
                ["Jwt:Audience"] = "NiramoyHealthStorefront",
                ["Jwt:AccessTokenExpirationMinutes"] = "120",
                ["Storage:LocalPath"] = "storage/test_uploads"
            });
        });
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string role = RoleNames.Customer)
    {
        var client = CreateClient();
        var uniqueSuffix = Random.Shared.Next(100000, 999999);
        var phone = $"0179{uniqueSuffix}";
        var password = "TestPassword2026!";

        // Register user
        var regRequest = new RegisterRequest(
            PhoneNumber: phone,
            FullName: $"Test User {uniqueSuffix}",
            Email: $"user{uniqueSuffix}@example.com",
            Password: password
        );

        var regContent = new StringContent(JsonSerializer.Serialize(regRequest), Encoding.UTF8, "application/json");
        var regResponse = await client.PostAsync("/api/v1/auth/register", regContent);
        regResponse.EnsureSuccessStatusCode();

        var regJson = await regResponse.Content.ReadAsStringAsync();
        using var regDoc = JsonDocument.Parse(regJson);
        var userId = regDoc.RootElement.GetProperty("data").GetProperty("user").GetProperty("id").GetInt64();
        var token = regDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();

        // If a staff role is requested, elevate user in test database
        if (role != RoleNames.Customer)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NiramoyDbContext>();
            var targetRole = db.Roles.FirstOrDefault(r => r.Name == role);
            if (targetRole != null)
            {
                db.UserRoles.Add(new NiramoyHealth.Domain.Entities.UserRole
                {
                    UserId = userId,
                    RoleId = targetRole.Id,
                    AssignedAt = DateTimeOffset.UtcNow
                });
                await db.SaveChangesAsync();

                // Re-login to get updated JWT claims with elevated role
                var loginReq = new LoginRequest(phone, password);
                var loginContent = new StringContent(JsonSerializer.Serialize(loginReq), Encoding.UTF8, "application/json");
                var loginResp = await client.PostAsync("/api/v1/auth/login", loginContent);
                loginResp.EnsureSuccessStatusCode();
                var loginJson = await loginResp.Content.ReadAsStringAsync();
                using var loginDoc = JsonDocument.Parse(loginJson);
                token = loginDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
            }
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
