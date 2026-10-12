using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Infrastructure.Identity;
using NiramoyHealth.Infrastructure.Payments;
using NiramoyHealth.Infrastructure.Persistence;
using NiramoyHealth.Infrastructure.Storage;

namespace NiramoyHealth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=127.0.0.1;Port=5432;Database=niramoy_health_dev;Username=niramoy_user;Password=niramoy_dev_pass_2026;Pooling=true;";

        services.AddDbContext<NiramoyDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<INiramoyDbContext>(sp => sp.GetRequiredService<NiramoyDbContext>());

        // Identity
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Storage
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Payments
        services.AddScoped<IPaymentGateway, BkashPaymentGateway>();
        services.AddScoped<IPaymentGateway, NagadPaymentGateway>();
        services.AddScoped<IPaymentGateway, CodPaymentGateway>();

        return services;
    }
}
