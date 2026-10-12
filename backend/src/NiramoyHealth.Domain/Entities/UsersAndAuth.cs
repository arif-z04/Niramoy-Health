using NiramoyHealth.Domain.Common;

namespace NiramoyHealth.Domain.Entities;

public class Division : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<District> Districts { get; set; } = new List<District>();
}

public class District : BaseEntity<int>
{
    public int DivisionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Division? Division { get; set; }
    public ICollection<Upazila> Upazilas { get; set; } = new List<Upazila>();
}

public class Upazila : BaseEntity<int>
{
    public int DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public string? PostCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public District? District { get; set; }
}

public class Role : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class User : BaseAuditableEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsPhoneVerified { get; set; }
    public bool IsEmailVerified { get; set; }
    public string Status { get; set; } = "active";
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockoutUntil { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public UserProfile? Profile { get; set; }
    public NotificationPreference? NotificationPreference { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

public class UserRole
{
    public long UserId { get; set; }
    public int RoleId { get; set; }
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public long? AssignedBy { get; set; }

    public User? User { get; set; }
    public Role? Role { get; set; }
}

public class UserProfile
{
    public long UserId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? AvatarUrl { get; set; }
    public string PreferredLanguage { get; set; } = "bn";
    public bool MarketingOptIn { get; set; } = true;
    public string? MedicalNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}

public class CustomerAddress : BaseAuditableEntity
{
    public long UserId { get; set; }
    public string AddressLabel { get; set; } = "Home";
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public int DivisionId { get; set; }
    public int DistrictId { get; set; }
    public int? UpazilaId { get; set; }
    public string AreaDetails { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? DeliveryInstructions { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
    public Division? Division { get; set; }
    public District? District { get; set; }
    public Upazila? Upazila { get; set; }
}

public class UserSession : BaseEntity
{
    public long UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}
