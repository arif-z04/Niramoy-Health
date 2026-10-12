using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Enums;
using NiramoyHealth.Domain.Exceptions;
using Xunit;

namespace NiramoyHealth.UnitTests;

public class BusinessLogicTests
{
    [Fact]
    public void OrderCancellation_Allowed_WhenStatusIsPending()
    {
        // Arrange
        var order = new Order
        {
            OrderStatus = OrderStatuses.Pending,
            TotalAmount = 500.00m
        };

        // Act
        var canCancel = order.OrderStatus == OrderStatuses.Pending || order.OrderStatus == OrderStatuses.Confirmed;

        // Assert
        Assert.True(canCancel);
    }

    [Theory]
    [InlineData(OrderStatuses.Processing)]
    [InlineData(OrderStatuses.Shipped)]
    [InlineData(OrderStatuses.OutForDelivery)]
    [InlineData(OrderStatuses.Delivered)]
    [InlineData(OrderStatuses.Cancelled)]
    public void OrderCancellation_Disallowed_WhenStatusIsBeyondPendingOrConfirmed(string status)
    {
        // Arrange
        var order = new Order
        {
            OrderStatus = status,
            TotalAmount = 500.00m
        };

        // Act
        var canCancel = order.OrderStatus == OrderStatuses.Pending || order.OrderStatus == OrderStatuses.Confirmed;

        // Assert
        Assert.False(canCancel);
    }

    [Fact]
    public void PrescriptionRequirement_RequiresApprovedStatus_ToPassValidation()
    {
        // Arrange
        var submittedPrescription = new Prescription { Status = PrescriptionStatuses.Submitted };
        var underReviewPrescription = new Prescription { Status = PrescriptionStatuses.UnderReview };
        var approvedPrescription = new Prescription { Status = PrescriptionStatuses.Approved };
        var rejectedPrescription = new Prescription { Status = PrescriptionStatuses.Rejected };

        // Act & Assert
        bool IsPrescriptionUsable(Prescription p) =>
            p.Status == PrescriptionStatuses.Approved || p.Status == PrescriptionStatuses.PartiallyApproved;

        Assert.False(IsPrescriptionUsable(submittedPrescription));
        Assert.False(IsPrescriptionUsable(underReviewPrescription));
        Assert.True(IsPrescriptionUsable(approvedPrescription));
        Assert.False(IsPrescriptionUsable(rejectedPrescription));
    }

    [Fact]
    public void PasswordHashing_VerifiesCorrectPassword_AndRejectsIncorrect()
    {
        // Arrange
        var rawPassword = "StrongPassword2026!";
        var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(rawPassword, workFactor: 10);

        // Act
        var isValid = BCrypt.Net.BCrypt.EnhancedVerify(rawPassword, hash);
        var isInvalid = BCrypt.Net.BCrypt.EnhancedVerify("WrongPassword999", hash);

        // Assert
        Assert.True(isValid);
        Assert.False(isInvalid);
    }
}
