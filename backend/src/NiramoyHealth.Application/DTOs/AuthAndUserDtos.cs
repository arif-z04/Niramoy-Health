namespace NiramoyHealth.Application.DTOs;

public record RegisterRequest(
    string PhoneNumber,
    string FullName,
    string? Email,
    string Password
);

public record LoginRequest(
    string? Identifier, // Phone or Email
    string Password,
    string? PhoneOrEmail = null
)
{
    public string EffectiveIdentifier => (!string.IsNullOrWhiteSpace(Identifier) ? Identifier : PhoneOrEmail ?? "").Trim();
};

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresInSeconds,
    UserDto User
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record ForgotPasswordRequest(
    string Identifier
);

public record ResetPasswordRequest(
    string Identifier,
    string ResetToken,
    string NewPassword
);

public record UserDto(
    long Id,
    Guid PublicId,
    string PhoneNumber,
    string? Email,
    string FullName,
    string Status,
    bool IsPhoneVerified,
    bool IsEmailVerified,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAt
);

public record UserProfileDto(
    long UserId,
    DateOnly? DateOfBirth,
    string? Gender,
    string? BloodGroup,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? AvatarUrl,
    string PreferredLanguage,
    bool MarketingOptIn,
    string? MedicalNotes
);

public record UpdateProfileRequest(
    DateOnly? DateOfBirth,
    string? Gender,
    string? BloodGroup,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? AvatarUrl,
    string PreferredLanguage,
    bool MarketingOptIn,
    string? MedicalNotes
);

public record AddressDto(
    long Id,
    long UserId,
    string AddressLabel,
    string RecipientName,
    string RecipientPhone,
    int DivisionId,
    string DivisionName,
    string DivisionNameBn,
    int DistrictId,
    string DistrictName,
    string DistrictNameBn,
    int? UpazilaId,
    string? UpazilaName,
    string? UpazilaNameBn,
    string AreaDetails,
    string? PostalCode,
    string? DeliveryInstructions,
    bool IsDefault,
    bool IsActive
);

public record CreateAddressRequest(
    string AddressLabel,
    string RecipientName,
    string RecipientPhone,
    int DivisionId,
    int DistrictId,
    int? UpazilaId,
    string AreaDetails,
    string? PostalCode,
    string? DeliveryInstructions,
    bool IsDefault
);

public record UpdateAddressRequest(
    string AddressLabel,
    string RecipientName,
    string RecipientPhone,
    int DivisionId,
    int DistrictId,
    int? UpazilaId,
    string AreaDetails,
    string? PostalCode,
    string? DeliveryInstructions,
    bool IsDefault
);

public record DivisionDto(
    int Id,
    string Code,
    string Name,
    string NameBn
);

public record DistrictDto(
    int Id,
    int DivisionId,
    string Code,
    string Name,
    string NameBn
);

public record UpazilaDto(
    int Id,
    int DistrictId,
    string Name,
    string NameBn,
    string? PostCode
);
