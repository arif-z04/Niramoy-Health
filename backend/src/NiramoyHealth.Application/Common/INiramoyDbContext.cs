using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NiramoyHealth.Domain.Entities;

namespace NiramoyHealth.Application.Common;

public interface INiramoyDbContext
{
    // Users & Auth
    DbSet<Division> Divisions { get; }
    DbSet<District> Districts { get; }
    DbSet<Upazila> Upazilas { get; }
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<CustomerAddress> CustomerAddresses { get; }
    DbSet<UserSession> UserSessions { get; }

    // Catalog
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Manufacturer> Manufacturers { get; }
    DbSet<Generic> Generics { get; }
    DbSet<DosageForm> DosageForms { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }
    DbSet<ProductReview> ProductReviews { get; }

    // Inventory
    DbSet<Warehouse> Warehouses { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<MedicineBatch> MedicineBatches { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }

    // Carts & Wishlists
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Wishlist> Wishlists { get; }
    DbSet<WishlistItem> WishlistItems { get; }

    // Prescriptions
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionFile> PrescriptionFiles { get; }
    DbSet<PrescriptionReviewRecord> PrescriptionReviewRecords { get; }

    // Orders & Payments
    DbSet<DeliveryMethod> DeliveryMethods { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderAddress> OrderAddresses { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<Refund> Refunds { get; }

    // Promotions & Marketing
    DbSet<DeliveryZone> DeliveryZones { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<CouponUsage> CouponUsages { get; }
    DbSet<PromotionalCampaign> PromotionalCampaigns { get; }
    DbSet<Banner> Banners { get; }
    DbSet<HomepageSection> HomepageSections { get; }
    DbSet<NewsletterSubscriber> NewsletterSubscribers { get; }

    // Notifications & Support
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }
    DbSet<SupportTicket> SupportTickets { get; }
    DbSet<SupportMessage> SupportMessages { get; }

    // Administration & Auditing
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<AdministrativeAction> AdministrativeActions { get; }
    DbSet<AppConfiguration> AppConfigurations { get; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
