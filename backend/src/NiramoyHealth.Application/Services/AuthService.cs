using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Enums;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class AuthService : IAuthService
{
    private readonly INiramoyDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtService;

    public AuthService(INiramoyDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwtService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var normalizedPhone = NormalizePhone(request.PhoneNumber);
        var existingPhone = await _db.Users.AnyAsync(u => u.PhoneNumber == normalizedPhone, ct);
        if (existingPhone)
        {
            throw new ConflictException($"User with phone number '{normalizedPhone}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var existingEmail = await _db.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail, ct);
            if (existingEmail)
            {
                throw new ConflictException($"User with email '{request.Email}' already exists.");
            }
        }

        var customerRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Customer, ct);
        if (customerRole == null)
        {
            customerRole = new Role { Name = RoleNames.Customer, DisplayName = "Customer", Description = "Storefront Customer" };
            _db.Roles.Add(customerRole);
            await _db.SaveChangesAsync(ct);
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            PhoneNumber = normalizedPhone,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            PasswordHash = passwordHash,
            FullName = request.FullName.Trim(),
            Status = "active",
            IsPhoneVerified = false,
            IsEmailVerified = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        _db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = customerRole.Id,
            AssignedAt = DateTimeOffset.UtcNow
        });

        _db.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            PreferredLanguage = "bn",
            MarketingOptIn = true,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        // Generate session & token
        var refreshToken = _jwtService.GenerateRefreshToken();
        var session = new UserSession
        {
            UserId = user.Id,
            TokenHash = refreshToken,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.UserSessions.Add(session);

        await _db.SaveChangesAsync(ct);

        var roles = new[] { RoleNames.Customer };
        var (accessToken, expiresIn) = _jwtService.GenerateAccessToken(user, roles);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            TokenType: "Bearer",
            ExpiresInSeconds: expiresIn,
            User: MapToUserDto(user, roles)
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var identifier = request.EffectiveIdentifier;
        var normalizedPhone = NormalizePhone(identifier);

        var phonePlus = "+88" + normalizedPhone;
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone 
                                   || u.PhoneNumber == phonePlus 
                                   || (u.Email != null && u.Email.ToLower() == identifier.ToLower()), ct);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid phone number/email or password.");
        }

        if (user.Status != "active")
        {
            throw new ForbiddenException($"User account is {user.Status}. Please contact customer support.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList();
        if (roles.Count == 0)
        {
            roles.Add(RoleNames.Customer);
        }

        var refreshToken = _jwtService.GenerateRefreshToken();
        var session = new UserSession
        {
            UserId = user.Id,
            TokenHash = refreshToken,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.UserSessions.Add(session);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        var (accessToken, expiresIn) = _jwtService.GenerateAccessToken(user, roles);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            TokenType: "Bearer",
            ExpiresInSeconds: expiresIn,
            User: MapToUserDto(user, roles)
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var session = await _db.UserSessions
            .Include(s => s.User)
            .ThenInclude(u => u!.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(s => s.TokenHash == refreshToken, ct);

        if (session == null || session.IsRevoked || session.ExpiresAt <= DateTimeOffset.UtcNow || session.User == null)
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        if (session.User.Status != "active")
        {
            throw new ForbiddenException($"User account is {session.User.Status}.");
        }

        // Revoke current session
        session.IsRevoked = true;

        // Create new session (token rotation)
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newSession = new UserSession
        {
            UserId = session.UserId,
            TokenHash = newRefreshToken,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            IsRevoked = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.UserSessions.Add(newSession);

        await _db.SaveChangesAsync(ct);

        var roles = session.User.UserRoles.Select(ur => ur.Role!.Name).ToList();
        var (accessToken, expiresIn) = _jwtService.GenerateAccessToken(session.User, roles);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken,
            TokenType: "Bearer",
            ExpiresInSeconds: expiresIn,
            User: MapToUserDto(session.User, roles)
        );
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var session = await _db.UserSessions.FirstOrDefaultAsync(s => s.TokenHash == refreshToken, ct);
        if (session != null && !session.IsRevoked)
        {
            session.IsRevoked = true;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(long userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        var roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList();
        return MapToUserDto(user, roles);
    }

    public async Task<UserProfileDto> GetUserProfileAsync(long userId, CancellationToken ct = default)
    {
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = userId,
                PreferredLanguage = "bn",
                MarketingOptIn = true,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync(ct);
        }

        return new UserProfileDto(
            UserId: profile.UserId,
            DateOfBirth: profile.DateOfBirth,
            Gender: profile.Gender,
            BloodGroup: profile.BloodGroup,
            EmergencyContactName: profile.EmergencyContactName,
            EmergencyContactPhone: profile.EmergencyContactPhone,
            AvatarUrl: profile.AvatarUrl,
            PreferredLanguage: profile.PreferredLanguage,
            MarketingOptIn: profile.MarketingOptIn,
            MedicalNotes: profile.MedicalNotes
        );
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(long userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile == null)
        {
            profile = new UserProfile { UserId = userId };
            _db.UserProfiles.Add(profile);
        }

        profile.DateOfBirth = request.DateOfBirth;
        profile.Gender = request.Gender;
        profile.BloodGroup = request.BloodGroup;
        profile.EmergencyContactName = request.EmergencyContactName;
        profile.EmergencyContactPhone = request.EmergencyContactPhone;
        profile.AvatarUrl = request.AvatarUrl;
        profile.PreferredLanguage = string.IsNullOrWhiteSpace(request.PreferredLanguage) ? "bn" : request.PreferredLanguage;
        profile.MarketingOptIn = request.MarketingOptIn;
        profile.MedicalNotes = request.MedicalNotes;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new UserProfileDto(
            UserId: profile.UserId,
            DateOfBirth: profile.DateOfBirth,
            Gender: profile.Gender,
            BloodGroup: profile.BloodGroup,
            EmergencyContactName: profile.EmergencyContactName,
            EmergencyContactPhone: profile.EmergencyContactPhone,
            AvatarUrl: profile.AvatarUrl,
            PreferredLanguage: profile.PreferredLanguage,
            MarketingOptIn: profile.MarketingOptIn,
            MedicalNotes: profile.MedicalNotes
        );
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "CurrentPassword", new[] { "Current password does not match." } }
            });
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        // Revoke existing sessions
        var sessions = await _db.UserSessions.Where(s => s.UserId == userId && !s.IsRevoked).ToListAsync(ct);
        foreach (var session in sessions)
        {
            session.IsRevoked = true;
        }

        await _db.SaveChangesAsync(ct);
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("880"))
        {
            digits = "0" + digits[3..];
        }
        else if (!digits.StartsWith("0") && digits.Length == 10)
        {
            digits = "0" + digits;
        }
        return digits;
    }

    private static UserDto MapToUserDto(User user, IReadOnlyList<string> roles) =>
        new(
            Id: user.Id,
            PublicId: user.PublicId,
            PhoneNumber: user.PhoneNumber,
            Email: user.Email,
            FullName: user.FullName,
            Status: user.Status,
            IsPhoneVerified: user.IsPhoneVerified,
            IsEmailVerified: user.IsEmailVerified,
            Roles: roles,
            CreatedAt: user.CreatedAt
        );
}
