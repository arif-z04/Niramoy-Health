using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Domain.Entities;

namespace NiramoyHealth.Infrastructure.Persistence;

public class NiramoyDbContext : DbContext, INiramoyDbContext
{
    public NiramoyDbContext(DbContextOptions<NiramoyDbContext> options) : base(options) { }

    // Users & Auth
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Upazila> Upazilas => Set<Upazila>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    // Catalog
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Generic> Generics => Set<Generic>();
    public DbSet<DosageForm> DosageForms => Set<DosageForm>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    // Inventory
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<MedicineBatch> MedicineBatches => Set<MedicineBatch>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    // Carts & Wishlists
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // Prescriptions
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionFile> PrescriptionFiles => Set<PrescriptionFile>();
    public DbSet<PrescriptionReviewRecord> PrescriptionReviewRecords => Set<PrescriptionReviewRecord>();

    // Orders & Payments
    public DbSet<DeliveryMethod> DeliveryMethods => Set<DeliveryMethod>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderAddress> OrderAddresses => Set<OrderAddress>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Refund> Refunds => Set<Refund>();

    // Promotions & Marketing
    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
    public DbSet<PromotionalCampaign> PromotionalCampaigns => Set<PromotionalCampaign>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<HomepageSection> HomepageSections => Set<HomepageSection>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();

    // Notifications & Support
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();

    // Administration & Auditing
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AdministrativeAction> AdministrativeActions => Set<AdministrativeAction>();
    public DbSet<AppConfiguration> AppConfigurations => Set<AppConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -------------------------------------------------------------
        // Users & Geography
        // -------------------------------------------------------------
        modelBuilder.Entity<Division>(entity =>
        {
            entity.ToTable("divisions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(20);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(100);
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("districts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.DivisionId).HasColumnName("division_id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(20);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(100);
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Division).WithMany(d => d.Districts).HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Upazila>(entity =>
        {
            entity.ToTable("upazilas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(100);
            entity.Property(e => e.PostCode).HasColumnName("post_code").HasMaxLength(10);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.District).WithMany(d => d.Upazilas).HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.PublicId).HasColumnName("public_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(150);
            entity.Property(e => e.IsPhoneVerified).HasColumnName("is_phone_verified");
            entity.Property(e => e.IsEmailVerified).HasColumnName("is_email_verified");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(e => e.FailedLoginAttempts).HasColumnName("failed_login_attempts");
            entity.Property(e => e.LockoutUntil).HasColumnName("lockout_until");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasOne(e => e.Profile).WithOne(p => p.User).HasForeignKey<UserProfile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.NotificationPreference).WithOne(np => np.User).HasForeignKey<NotificationPreference>(np => np.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");

            entity.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id").ValueGeneratedNever();
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(20);
            entity.Property(e => e.BloodGroup).HasColumnName("blood_group").HasMaxLength(10);
            entity.Property(e => e.EmergencyContactName).HasColumnName("emergency_contact_name").HasMaxLength(150);
            entity.Property(e => e.EmergencyContactPhone).HasColumnName("emergency_contact_phone").HasMaxLength(20);
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.PreferredLanguage).HasColumnName("preferred_language").HasMaxLength(5);
            entity.Property(e => e.MarketingOptIn).HasColumnName("marketing_opt_in");
            entity.Property(e => e.MedicalNotes).HasColumnName("medical_notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.ToTable("customer_addresses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AddressLabel).HasColumnName("address_label").HasMaxLength(50);
            entity.Property(e => e.RecipientName).HasColumnName("recipient_name").HasMaxLength(150);
            entity.Property(e => e.RecipientPhone).HasColumnName("recipient_phone").HasMaxLength(20);
            entity.Property(e => e.DivisionId).HasColumnName("division_id");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.UpazilaId).HasColumnName("upazila_id");
            entity.Property(e => e.AreaDetails).HasColumnName("area_details");
            entity.Property(e => e.PostalCode).HasColumnName("postal_code").HasMaxLength(10);
            entity.Property(e => e.DeliveryInstructions).HasColumnName("delivery_instructions");
            entity.Property(e => e.Latitude).HasColumnName("latitude").HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasColumnName("longitude").HasPrecision(9, 6);
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User).WithMany(u => u.Addresses).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.District).WithMany().HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Upazila).WithMany().HasForeignKey(e => e.UpazilaId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("user_sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TokenHash).HasColumnName("token_hash").HasMaxLength(255);
            entity.Property(e => e.DeviceInfo).HasColumnName("device_info").HasMaxLength(255);
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.IsRevoked).HasColumnName("is_revoked");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.User).WithMany(u => u.Sessions).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------------------------------------------------
        // Catalog
        // -------------------------------------------------------------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(180);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(150);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IconUrl).HasColumnName("icon_url");
            entity.Property(e => e.BannerUrl).HasColumnName("banner_url");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Parent).WithMany(c => c.SubCategories).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("brands");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(180);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(150);
            entity.Property(e => e.LogoUrl).HasColumnName("logo_url");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.WebsiteUrl).HasColumnName("website_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.ToTable("manufacturers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(220);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(200);
            entity.Property(e => e.Country).HasColumnName("country").HasMaxLength(100);
            entity.Property(e => e.LicenseNumber).HasColumnName("license_number").HasMaxLength(100);
            entity.Property(e => e.WebsiteUrl).HasColumnName("website_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Generic>(entity =>
        {
            entity.ToTable("generics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(220);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.TherapeuticClass).HasColumnName("therapeutic_class").HasMaxLength(150);
            entity.Property(e => e.Indication).HasColumnName("indication");
            entity.Property(e => e.DosageGuidelines).HasColumnName("dosage_guidelines");
            entity.Property(e => e.SideEffects).HasColumnName("side_effects");
            entity.Property(e => e.Precautions).HasColumnName("precautions");
            entity.Property(e => e.PregnancyCategory).HasColumnName("pregnancy_category").HasMaxLength(10);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<DosageForm>(entity =>
        {
            entity.ToTable("dosage_forms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.PublicId).HasColumnName("public_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(100);
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(255);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(255);
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(e => e.GenericId).HasColumnName("generic_id");
            entity.Property(e => e.DosageFormId).HasColumnName("dosage_form_id");
            entity.Property(e => e.Strength).HasColumnName("strength").HasMaxLength(100);
            entity.Property(e => e.PackSize).HasColumnName("pack_size").HasMaxLength(100);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(50);
            entity.Property(e => e.UnitsPerPack).HasColumnName("units_per_pack");
            entity.Property(e => e.BasePrice).HasColumnName("base_price").HasPrecision(12, 2);
            entity.Property(e => e.DiscountPrice).HasColumnName("discount_price").HasPrecision(12, 2);
            entity.Property(e => e.IsPrescriptionRequired).HasColumnName("is_prescription_required");
            entity.Property(e => e.IsOtc).HasColumnName("is_otc");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DescriptionBn).HasColumnName("description_bn");
            entity.Property(e => e.UsageInstructions).HasColumnName("usage_instructions");
            entity.Property(e => e.StorageConditions).HasColumnName("storage_conditions");
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Badge).HasColumnName("badge").HasMaxLength(50);
            entity.Property(e => e.AvgRating).HasColumnName("avg_rating").HasPrecision(3, 2);
            entity.Property(e => e.ReviewCount).HasColumnName("review_count");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasOne(e => e.Category).WithMany(c => c.Products).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Brand).WithMany(b => b.Products).HasForeignKey(e => e.BrandId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Manufacturer).WithMany(m => m.Products).HasForeignKey(e => e.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Generic).WithMany(g => g.Products).HasForeignKey(e => e.GenericId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.DosageForm).WithMany(df => df.Products).HasForeignKey(e => e.DosageFormId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("product_variants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(100);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(150);
            entity.Property(e => e.VariantAttributesJson).HasColumnName("variant_attributes").HasColumnType("jsonb");
            entity.Property(e => e.BasePrice).HasColumnName("base_price").HasPrecision(12, 2);
            entity.Property(e => e.DiscountPrice).HasColumnName("discount_price").HasPrecision(12, 2);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Product).WithMany(p => p.Variants).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("product_categories");
            entity.HasKey(e => new { e.ProductId, e.CategoryId });
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");

            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.AltText).HasColumnName("alt_text").HasMaxLength(255);
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Product).WithMany(p => p.Images).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductAttribute>(entity =>
        {
            entity.ToTable("product_attributes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.AttributeKey).HasColumnName("attribute_key").HasMaxLength(100);
            entity.Property(e => e.AttributeValue).HasColumnName("attribute_value");
            entity.Property(e => e.AttributeValueBn).HasColumnName("attribute_value_bn");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");

            entity.HasOne(e => e.Product).WithMany(p => p.Attributes).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.ToTable("product_reviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(200);
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.IsVerifiedPurchase).HasColumnName("is_verified_purchase");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.ModeratedBy).HasColumnName("moderated_by");
            entity.Property(e => e.ModeratedAt).HasColumnName("moderated_at");
            entity.Property(e => e.ModerationNote).HasColumnName("moderation_note");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Product).WithMany(p => p.Reviews).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------------------------------------------------
        // Inventory
        // -------------------------------------------------------------
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("warehouses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150);
            entity.Property(e => e.DivisionId).HasColumnName("division_id");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            entity.Property(e => e.IsCentralHub).HasColumnName("is_central_hub");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.District).WithMany().HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.QuantityOnHand).HasColumnName("quantity_on_hand");
            entity.Property(e => e.QuantityReserved).HasColumnName("quantity_reserved");
            entity.Property(e => e.ReorderLevel).HasColumnName("reorder_level");
            entity.Property(e => e.ReorderQuantity).HasColumnName("reorder_quantity");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Product).WithMany(p => p.Inventories).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Warehouse).WithMany(w => w.Inventories).HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedicineBatch>(entity =>
        {
            entity.ToTable("medicine_batches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.BatchNumber).HasColumnName("batch_number").HasMaxLength(100);
            entity.Property(e => e.MfgDate).HasColumnName("mfg_date");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.QuantityReceived).HasColumnName("quantity_received");
            entity.Property(e => e.QuantityRemaining).HasColumnName("quantity_remaining");
            entity.Property(e => e.UnitCost).HasColumnName("unit_cost").HasPrecision(12, 2);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Warehouse).WithMany().HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.ToTable("inventory_transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.TransactionType).HasColumnName("transaction_type").HasMaxLength(50);
            entity.Property(e => e.QuantityChange).HasColumnName("quantity_change");
            entity.Property(e => e.BalanceAfter).HasColumnName("balance_after");
            entity.Property(e => e.ReferenceType).HasColumnName("reference_type").HasMaxLength(50);
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id").HasMaxLength(100);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Warehouse).WithMany().HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Batch).WithMany().HasForeignKey(e => e.BatchId).OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------------------------------------------------
        // Carts & Wishlists
        // -------------------------------------------------------------
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("carts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.PublicId).HasColumnName("public_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.GuestToken).HasColumnName("guest_token");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.AddedAt).HasColumnName("added_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Cart).WithMany(c => c.Items).HasForeignKey(e => e.CartId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.ToTable("wishlists");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.ToTable("wishlist_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.WishlistId).HasColumnName("wishlist_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.AddedAt).HasColumnName("added_at");

            entity.HasOne(e => e.Wishlist).WithMany(w => w.Items).HasForeignKey(e => e.WishlistId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------------------------------------------------
        // Prescriptions
        // -------------------------------------------------------------
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.ToTable("prescriptions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.PublicId).HasColumnName("public_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PatientName).HasColumnName("patient_name").HasMaxLength(150);
            entity.Property(e => e.PatientAge).HasColumnName("patient_age");
            entity.Property(e => e.PatientGender).HasColumnName("patient_gender").HasMaxLength(20);
            entity.Property(e => e.DoctorName).HasColumnName("doctor_name").HasMaxLength(150);
            entity.Property(e => e.HospitalClinic).HasColumnName("hospital_clinic").HasMaxLength(200);
            entity.Property(e => e.PrescriptionDate).HasColumnName("prescription_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.CustomerNotes).HasColumnName("customer_notes");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User).WithMany(u => u.Prescriptions).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Reviewer).WithMany().HasForeignKey(e => e.ReviewedBy).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PrescriptionFile>(entity =>
        {
            entity.ToTable("prescription_files");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.PrescriptionId).HasColumnName("prescription_id");
            entity.Property(e => e.StorageProvider).HasColumnName("storage_provider").HasMaxLength(50);
            entity.Property(e => e.FileStorageKey).HasColumnName("file_storage_key").HasMaxLength(500);
            entity.Property(e => e.OriginalFilename).HasColumnName("original_filename").HasMaxLength(255);
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.MimeType).HasColumnName("mime_type").HasMaxLength(100);
            entity.Property(e => e.IsSecureStorage).HasColumnName("is_secure_storage");
            entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at");

            entity.HasOne(e => e.Prescription).WithMany(p => p.Files).HasForeignKey(e => e.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrescriptionReviewRecord>(entity =>
        {
            entity.ToTable("prescription_review_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.PrescriptionId).HasColumnName("prescription_id");
            entity.Property(e => e.PharmacistId).HasColumnName("pharmacist_id");
            entity.Property(e => e.PreviousStatus).HasColumnName("previous_status").HasMaxLength(30);
            entity.Property(e => e.NewStatus).HasColumnName("new_status").HasMaxLength(30);
            entity.Property(e => e.DecisionNotes).HasColumnName("decision_notes");
            entity.Property(e => e.VerifiedMedicinesJson).HasColumnName("verified_medicines").HasColumnType("jsonb");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");

            entity.HasOne(e => e.Prescription).WithMany(p => p.ReviewRecords).HasForeignKey(e => e.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Pharmacist).WithMany().HasForeignKey(e => e.PharmacistId).OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------------------------------------------------
        // Orders & Payments
        // -------------------------------------------------------------
        modelBuilder.Entity<DeliveryMethod>(entity =>
        {
            entity.ToTable("delivery_methods");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(100);
            entity.Property(e => e.BaseFee).HasColumnName("base_fee").HasPrecision(12, 2);
            entity.Property(e => e.FreeDeliveryThreshold).HasColumnName("free_delivery_threshold").HasPrecision(12, 2);
            entity.Property(e => e.EstimatedDaysMin).HasColumnName("estimated_days_min");
            entity.Property(e => e.EstimatedDaysMax).HasColumnName("estimated_days_max");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.ToTable("payment_methods");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.NameBn).HasColumnName("name_bn").HasMaxLength(100);
            entity.Property(e => e.IsMfs).HasColumnName("is_mfs");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.PublicId).HasColumnName("public_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number").HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PrescriptionId).HasColumnName("prescription_id");
            entity.Property(e => e.OrderStatus).HasColumnName("order_status").HasMaxLength(30);
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").HasMaxLength(30);
            entity.Property(e => e.SubtotalAmount).HasColumnName("subtotal_amount").HasPrecision(12, 2);
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasPrecision(12, 2);
            entity.Property(e => e.DeliveryFee).HasColumnName("delivery_fee").HasPrecision(12, 2);
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(12, 2);
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3);
            entity.Property(e => e.CouponCode).HasColumnName("coupon_code").HasMaxLength(50);
            entity.Property(e => e.DeliveryMethodId).HasColumnName("delivery_method_id");
            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");
            entity.Property(e => e.CustomerNotes).HasColumnName("customer_notes");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(128);
            entity.Property(e => e.PlacedAt).HasColumnName("placed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User).WithMany(u => u.Orders).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Prescription).WithMany().HasForeignKey(e => e.PrescriptionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.DeliveryMethod).WithMany().HasForeignKey(e => e.DeliveryMethodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PaymentMethod).WithMany().HasForeignKey(e => e.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.DeliveryAddress).WithOne(oa => oa.Order).HasForeignKey<OrderAddress>(oa => oa.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderAddress>(entity =>
        {
            entity.ToTable("order_addresses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.RecipientName).HasColumnName("recipient_name").HasMaxLength(150);
            entity.Property(e => e.RecipientPhone).HasColumnName("recipient_phone").HasMaxLength(20);
            entity.Property(e => e.Division).HasColumnName("division").HasMaxLength(100);
            entity.Property(e => e.District).HasColumnName("district").HasMaxLength(100);
            entity.Property(e => e.Upazila).HasColumnName("upazila").HasMaxLength(100);
            entity.Property(e => e.AreaDetails).HasColumnName("area_details");
            entity.Property(e => e.PostalCode).HasColumnName("postal_code").HasMaxLength(10);
            entity.Property(e => e.DeliveryInstructions).HasColumnName("delivery_instructions");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductVariantId).HasColumnName("product_variant_id");
            entity.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(255);
            entity.Property(e => e.ProductNameBn).HasColumnName("product_name_bn").HasMaxLength(255);
            entity.Property(e => e.ProductSku).HasColumnName("product_sku").HasMaxLength(100);
            entity.Property(e => e.UnitPriceSnapshot).HasColumnName("unit_price_snapshot").HasPrecision(12, 2);
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasPrecision(12, 2);
            entity.Property(e => e.IsPrescriptionItem).HasColumnName("is_prescription_item");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Order).WithMany(o => o.Items).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("order_status_history");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PreviousStatus).HasColumnName("previous_status").HasMaxLength(30);
            entity.Property(e => e.NewStatus).HasColumnName("new_status").HasMaxLength(30);
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Order).WithMany(o => o.StatusHistory).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.PublicId).HasColumnName("public_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").HasMaxLength(30);
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2);
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3);
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id").HasMaxLength(100);
            entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(128);
            entity.Property(e => e.GatewayProvider).HasColumnName("gateway_provider").HasMaxLength(50);
            entity.Property(e => e.GatewayResponseJson).HasColumnName("gateway_response").HasColumnType("jsonb");
            entity.Property(e => e.InitiatedAt).HasColumnName("initiated_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Order).WithMany(o => o.Payments).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PaymentMethod).WithMany().HasForeignKey(e => e.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("payment_transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.TransactionType).HasColumnName("transaction_type").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(e => e.ProviderReference).HasColumnName("provider_reference").HasMaxLength(150);
            entity.Property(e => e.RawResponseSanitizedJson).HasColumnName("raw_response_sanitized").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Payment).WithMany(p => p.Transactions).HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.ToTable("refunds");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.RefundAmount).HasColumnName("refund_amount").HasPrecision(12, 2);
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.MfsRefundTrxId).HasColumnName("mfs_refund_trx_id").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Order).WithMany(o => o.Refunds).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Payment).WithMany().HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------------------------------------------------
        // Delivery Zones & Promotions
        // -------------------------------------------------------------
        modelBuilder.Entity<DeliveryZone>(entity =>
        {
            entity.ToTable("delivery_zones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.DivisionId).HasColumnName("division_id");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.ZoneName).HasColumnName("zone_name").HasMaxLength(150);
            entity.Property(e => e.EstimatedDeliveryHours).HasColumnName("estimated_delivery_hours");
            entity.Property(e => e.BaseDeliveryFee).HasColumnName("base_delivery_fee").HasPrecision(12, 2);
            entity.Property(e => e.IsExpressAvailable).HasColumnName("is_express_available");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.District).WithMany().HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("coupons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50);
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(150);
            entity.Property(e => e.TitleBn).HasColumnName("title_bn").HasMaxLength(150);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountType).HasColumnName("discount_type").HasMaxLength(20);
            entity.Property(e => e.DiscountValue).HasColumnName("discount_value").HasPrecision(12, 2);
            entity.Property(e => e.MaxDiscountAmount).HasColumnName("max_discount_amount").HasPrecision(12, 2);
            entity.Property(e => e.MinOrderAmount).HasColumnName("min_order_amount").HasPrecision(12, 2);
            entity.Property(e => e.UsageLimitPerUser).HasColumnName("usage_limit_per_user");
            entity.Property(e => e.TotalUsageLimit).HasColumnName("total_usage_limit");
            entity.Property(e => e.TimesUsed).HasColumnName("times_used");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.ToTable("coupon_usages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.DiscountApplied).HasColumnName("discount_applied").HasPrecision(12, 2);
            entity.Property(e => e.UsedAt).HasColumnName("used_at");

            entity.HasOne(e => e.Coupon).WithMany(c => c.Usages).HasForeignKey(e => e.CouponId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PromotionalCampaign>(entity =>
        {
            entity.ToTable("promotional_campaigns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(100);
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(150);
            entity.Property(e => e.TitleBn).HasColumnName("title_bn").HasMaxLength(150);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.BannerUrl).HasColumnName("banner_url");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.ToTable("banners");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(150);
            entity.Property(e => e.Subtitle).HasColumnName("subtitle").HasMaxLength(255);
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.TargetUrl).HasColumnName("target_url");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Campaign).WithMany(c => c.Banners).HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<HomepageSection>(entity =>
        {
            entity.ToTable("homepage_sections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.SectionType).HasColumnName("section_type").HasMaxLength(50);
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(150);
            entity.Property(e => e.TitleBn).HasColumnName("title_bn").HasMaxLength(150);
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ConfigurationJson).HasColumnName("configuration").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<NewsletterSubscriber>(entity =>
        {
            entity.ToTable("newsletter_subscribers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.SubscribedAt).HasColumnName("subscribed_at");
        });

        // -------------------------------------------------------------
        // Notifications & Support
        // -------------------------------------------------------------
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(200);
            entity.Property(e => e.TitleBn).HasColumnName("title_bn").HasMaxLength(200);
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.MessageBn).HasColumnName("message_bn");
            entity.Property(e => e.NotificationType).HasColumnName("notification_type").HasMaxLength(50);
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id").HasMaxLength(100);
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.ToTable("notification_preferences");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id").ValueGeneratedNever();
            entity.Property(e => e.SmsOrderUpdates).HasColumnName("sms_order_updates");
            entity.Property(e => e.SmsPromotions).HasColumnName("sms_promotions");
            entity.Property(e => e.EmailOrderUpdates).HasColumnName("email_order_updates");
            entity.Property(e => e.EmailPromotions).HasColumnName("email_promotions");
            entity.Property(e => e.PushNotifications).HasColumnName("push_notifications");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.ToTable("support_tickets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.TicketNumber).HasColumnName("ticket_number").HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(200);
            entity.Property(e => e.Priority).HasColumnName("priority").HasMaxLength(20);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SupportMessage>(entity =>
        {
            entity.ToTable("support_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.IsStaffReply).HasColumnName("is_staff_reply");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Ticket).WithMany(t => t.Messages).HasForeignKey(e => e.TicketId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Sender).WithMany().HasForeignKey(e => e.SenderId).OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------------------------------------------------
        // Administration & Auditing
        // -------------------------------------------------------------
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(100);
            entity.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(100);
            entity.Property(e => e.EntityId).HasColumnName("entity_id").HasMaxLength(100);
            entity.Property(e => e.OldValuesJson).HasColumnName("old_values").HasColumnType("jsonb");
            entity.Property(e => e.NewValuesJson).HasColumnName("new_values").HasColumnType("jsonb");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<AdministrativeAction>(entity =>
        {
            entity.ToTable("administrative_actions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.AdminUserId).HasColumnName("admin_user_id");
            entity.Property(e => e.ActionType).HasColumnName("action_type").HasMaxLength(100);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.TargetUserId).HasColumnName("target_user_id");
            entity.Property(e => e.TargetOrderId).HasColumnName("target_order_id");
            entity.Property(e => e.TargetProductId).HasColumnName("target_product_id");
            entity.Property(e => e.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.AdminUser).WithMany().HasForeignKey(e => e.AdminUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppConfiguration>(entity =>
        {
            entity.ToTable("app_configurations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ConfigKey).HasColumnName("config_key").HasMaxLength(100);
            entity.Property(e => e.ConfigValue).HasColumnName("config_value");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsEncrypted).HasColumnName("is_encrypted");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
    }
}
