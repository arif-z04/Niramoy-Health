using System.Text.RegularExpressions;
using FluentValidation;
using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private static readonly Regex BdPhoneRegex = new(@"^(?:\+?880|0)?1[3-9]\d{8}$", RegexOptions.Compiled);

    public RegisterRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(BdPhoneRegex).WithMessage("A valid Bangladeshi phone number is required (e.g. 01712345678 or +8801712345678).");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Za-z]").WithMessage("Password must contain at least one letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Phone number or email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    private static readonly Regex BdPhoneRegex = new(@"^(?:\+?880|0)?1[3-9]\d{8}$", RegexOptions.Compiled);

    public CreateAddressRequestValidator()
    {
        RuleFor(x => x.RecipientName)
            .NotEmpty().WithMessage("Recipient name is required.");

        RuleFor(x => x.RecipientPhone)
            .NotEmpty().WithMessage("Recipient phone is required.")
            .Matches(BdPhoneRegex).WithMessage("Valid Bangladeshi recipient phone number is required.");

        RuleFor(x => x.DivisionId)
            .GreaterThan(0).WithMessage("Valid division is required.");

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("Valid district is required.");

        RuleFor(x => x.AreaDetails)
            .NotEmpty().WithMessage("Detailed street/area address is required.");
    }
}

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Invalid product ID.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
    }
}

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 100).WithMessage("Quantity must be between 1 and 100.");
    }
}

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.DeliveryMethodId)
            .GreaterThan(0).WithMessage("Delivery method is required.");

        RuleFor(x => x.PaymentMethodId)
            .GreaterThan(0).WithMessage("Payment method is required.");

        RuleFor(x => x)
            .Must(x => x.DeliveryAddressId.HasValue || x.NewAddress != null)
            .WithMessage("Either an existing address ID or new address details must be provided.");

        When(x => x.NewAddress != null, () =>
        {
            RuleFor(x => x.NewAddress!.RecipientName).NotEmpty().WithMessage("Recipient name is required.");
            RuleFor(x => x.NewAddress!.RecipientPhone).NotEmpty().WithMessage("Recipient phone is required.");
            RuleFor(x => x.NewAddress!.Division).NotEmpty().WithMessage("Division is required.");
            RuleFor(x => x.NewAddress!.District).NotEmpty().WithMessage("District is required.");
            RuleFor(x => x.NewAddress!.AreaDetails).NotEmpty().WithMessage("Area details are required.");
        });
    }
}

public class SubmitPrescriptionRequestValidator : AbstractValidator<SubmitPrescriptionRequest>
{
    public SubmitPrescriptionRequestValidator()
    {
        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Patient name is required.")
            .MaximumLength(150).WithMessage("Patient name cannot exceed 150 characters.");

        RuleFor(x => x.PatientAge)
            .InclusiveBetween(0, 130).WithMessage("Patient age must be between 0 and 130.")
            .When(x => x.PatientAge.HasValue);
    }
}

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(60).WithMessage("SKU cannot exceed 60 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(150).WithMessage("Slug cannot exceed 150 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category is required.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than 0.");

        RuleFor(x => x.DiscountPrice)
            .GreaterThan(0).WithMessage("Discount price must be greater than 0.")
            .LessThan(x => x.BasePrice).WithMessage("Discount price must be strictly less than base price.")
            .When(x => x.DiscountPrice.HasValue);
    }
}

public class CreateCouponRequestValidator : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(30).WithMessage("Coupon code cannot exceed 30 characters.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0.");

        RuleFor(x => x.DiscountType)
            .Must(t => t == "percentage" || t == "fixed")
            .WithMessage("Discount type must be either 'percentage' or 'fixed'.");

        RuleFor(x => x.ValidUntil)
            .GreaterThan(x => x.ValidFrom).WithMessage("Expiration date must be after start date.");
    }
}
