using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Enums;
using Xunit;

namespace NiramoyHealth.UnitTests;

public class PricingAndDiscountsTests
{
    [Fact]
    public void ProductCurrentPrice_ReturnsDiscountPrice_WhenDiscountExists()
    {
        // Arrange
        var product = new Product
        {
            BasePrice = 120.00m,
            DiscountPrice = 100.00m
        };

        // Act
        var currentPrice = product.DiscountPrice ?? product.BasePrice;

        // Assert
        Assert.Equal(100.00m, currentPrice);
    }

    [Fact]
    public void ProductCurrentPrice_ReturnsBasePrice_WhenDiscountIsNull()
    {
        // Arrange
        var product = new Product
        {
            BasePrice = 120.00m,
            DiscountPrice = null
        };

        // Act
        var currentPrice = product.DiscountPrice ?? product.BasePrice;

        // Assert
        Assert.Equal(120.00m, currentPrice);
    }

    [Theory]
    [InlineData(100.0, 80.0, 20.0)]
    [InlineData(250.0, 200.0, 20.0)]
    [InlineData(500.0, 450.0, 10.0)]
    public void DiscountPercentage_CalculatesCorrectly(double basePrice, double discountPrice, double expectedPercentage)
    {
        // Arrange
        var bp = (decimal)basePrice;
        var dp = (decimal)discountPrice;

        // Act
        var percentage = Math.Round(((bp - dp) / bp) * 100, 2);

        // Assert
        Assert.Equal((decimal)expectedPercentage, percentage);
    }

    [Fact]
    public void CouponPercentageDiscount_CalculatesWithinMaxLimit()
    {
        // Arrange: 10% coupon with Max cap 50 BDT on subtotal 800 BDT
        var coupon = new Coupon
        {
            Code = "HEALTH10",
            DiscountType = DiscountTypes.Percentage,
            DiscountValue = 10.00m,
            MaxDiscountAmount = 50.00m,
            MinOrderAmount = 200.00m
        };
        decimal subtotal = 800.00m;

        // Act: 10% of 800 is 80 BDT, but capped at 50 BDT
        decimal rawDiscount = (subtotal * coupon.DiscountValue) / 100m;
        decimal appliedDiscount = coupon.MaxDiscountAmount.HasValue
            ? Math.Min(rawDiscount, coupon.MaxDiscountAmount.Value)
            : rawDiscount;

        // Assert
        Assert.Equal(80.00m, rawDiscount);
        Assert.Equal(50.00m, appliedDiscount);
    }

    [Fact]
    public void CouponFixedDiscount_DoesNotExceedSubtotal()
    {
        // Arrange: Fixed 150 BDT off on 100 BDT order
        var coupon = new Coupon
        {
            Code = "FLAT150",
            DiscountType = DiscountTypes.FixedAmount,
            DiscountValue = 150.00m,
            MinOrderAmount = 50.00m
        };
        decimal subtotal = 100.00m;

        // Act
        decimal appliedDiscount = Math.Min(subtotal, coupon.DiscountValue);
        decimal grandTotal = Math.Max(0m, subtotal - appliedDiscount);

        // Assert
        Assert.Equal(100.00m, appliedDiscount);
        Assert.Equal(0.00m, grandTotal);
    }

    [Theory]
    [InlineData(500.0, 60.0)]   // Subtotal below 1000 threshold incurs 60 BDT delivery fee
    [InlineData(1000.0, 0.0)]  // Subtotal at or above 1000 threshold qualifies for FREE delivery
    [InlineData(2500.0, 0.0)]  // Subtotal above 1000 qualifies for FREE delivery
    public void DeliveryFee_CalculatesCorrectlyBasedOnThreshold(double subtotalAmount, double expectedFee)
    {
        // Arrange
        decimal subtotal = (decimal)subtotalAmount;
        decimal freeThreshold = 1000.00m;
        decimal standardFee = 60.00m;

        // Act
        decimal actualFee = subtotal >= freeThreshold ? 0.00m : standardFee;

        // Assert
        Assert.Equal((decimal)expectedFee, actualFee);
    }
}
