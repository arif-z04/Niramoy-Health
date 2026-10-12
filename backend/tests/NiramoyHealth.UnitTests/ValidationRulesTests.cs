using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Validators;
using Xunit;

namespace NiramoyHealth.UnitTests;

public class ValidationRulesTests
{
    private readonly RegisterRequestValidator _registerValidator = new();
    private readonly AddToCartRequestValidator _cartValidator = new();
    private readonly CreateProductRequestValidator _productValidator = new();

    [Theory]
    [InlineData("01712345678", true)]
    [InlineData("+8801812345678", true)]
    [InlineData("8801912345678", true)]
    [InlineData("01312345678", true)]
    [InlineData("01412345678", true)]
    [InlineData("01512345678", true)]
    [InlineData("01612345678", true)]
    [InlineData("01212345678", false)] // Invalid prefix 012
    [InlineData("0171234567", false)]  // 10 digits (too short)
    [InlineData("017123456789", false)] // 12 digits (too long)
    [InlineData("not-a-number", false)]
    public void RegisterValidator_ValidatesBangladeshiPhoneNumbers(string phone, bool expectedValid)
    {
        // Arrange
        var request = new RegisterRequest(
            PhoneNumber: phone,
            FullName: "Ashraful Islam",
            Email: "ashraf@example.com",
            Password: "SecurePassword123"
        );

        // Act
        var result = _registerValidator.Validate(request);

        // Assert
        if (expectedValid)
        {
            Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
        }
        else
        {
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.PhoneNumber));
        }
    }

    [Theory]
    [InlineData("short1", false)]       // Less than 8 chars
    [InlineData("onlylettershere", false)] // No numbers
    [InlineData("1234567890", false)]    // No letters
    [InlineData("ValidPass2026", true)]  // Valid letters and numbers, >= 8 chars
    public void RegisterValidator_ValidatesPasswordStrength(string password, bool expectedValid)
    {
        // Arrange
        var request = new RegisterRequest(
            PhoneNumber: "01712345678",
            FullName: "Valid Customer",
            Email: "valid@example.com",
            Password: password
        );

        // Act
        var result = _registerValidator.Validate(request);

        // Assert
        if (expectedValid)
        {
            Assert.True(result.IsValid);
        }
        else
        {
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password));
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(10, true)]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(101, false)]
    public void AddToCartValidator_ValidatesQuantityBounds(int quantity, bool expectedValid)
    {
        // Arrange
        var request = new AddToCartRequest(ProductId: 10, Quantity: quantity);

        // Act
        var result = _cartValidator.Validate(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void CreateProductValidator_RejectsDiscountPriceHigherThanBasePrice()
    {
        // Arrange: Discount price 150 > Base price 100
        var request = new CreateProductRequest(
            Sku: "NAPA-500",
            Slug: "napa-500mg-tablet",
            Name: "Napa 500mg Tablet",
            NameBn: "নাপা ৫০০ মি.গ্রা. ট্যাবলেট",
            CategoryId: 1,
            BrandId: 1,
            ManufacturerId: 1,
            GenericId: 1,
            DosageFormId: 1,
            Strength: "500mg",
            PackSize: "10 Strips",
            UnitOfMeasure: "Strip",
            UnitsPerPack: 10,
            BasePrice: 100.00m,
            DiscountPrice: 120.00m, // Invalid: discount price cannot be higher than base price
            IsPrescriptionRequired: false,
            IsOtc: true,
            Description: "Paracetamol 500mg tablet",
            DescriptionBn: "প্যারাসিটামল ৫০০ মিগ্রা",
            UsageInstructions: "1 tablet after meals",
            StorageConditions: "Store below 30C",
            IsFeatured: true,
            Badge: "Trending"
        );

        // Act
        var result = _productValidator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductRequest.DiscountPrice));
    }
}
