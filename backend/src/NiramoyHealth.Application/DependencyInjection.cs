using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Application.Services;
using NiramoyHealth.Application.Validators;

namespace NiramoyHealth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ISupportService, SupportService>();

        return services;
    }
}
