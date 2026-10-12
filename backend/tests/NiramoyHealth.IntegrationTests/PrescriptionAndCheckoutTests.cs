using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Domain.Enums;
using Xunit;

namespace NiramoyHealth.IntegrationTests;

public class PrescriptionAndCheckoutTests : IClassFixture<NiramoyTestWebApplicationFactory>
{
    private readonly NiramoyTestWebApplicationFactory _factory;

    public PrescriptionAndCheckoutTests(NiramoyTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PrescriptionUpload_InitializesStatusToSubmitted_NeverAutoApproved()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync();

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Rahim Uddin"), "PatientName");
        content.Add(new StringContent("45"), "PatientAge");
        content.Add(new StringContent("male"), "PatientGender");
        content.Add(new StringContent("Dr. Mahbubur Rahman"), "DoctorName");
        content.Add(new StringContent("Please deliver morning dose"), "CustomerNotes");

        var fileBytes = Encoding.UTF8.GetBytes("%PDF-1.4 Fake test prescription content for medical upload");
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "files", "prescription_test.pdf");

        // Act: Submit prescription
        var response = await client.PostAsync("/api/v1/prescriptions", content);

        // Assert: HTTP 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var presData = doc.RootElement.GetProperty("data");

        // CRITICAL: Must be default "submitted", NEVER "approved"
        var status = presData.GetProperty("status").GetString();
        Assert.Equal(PrescriptionStatuses.Submitted, status);
        Assert.Equal("Rahim Uddin", presData.GetProperty("patientName").GetString());
    }

    [Fact]
    public async Task PharmacistReview_UpdatesPrescriptionStatus_ToApproved()
    {
        // Arrange 1: Customer submits prescription
        var customerClient = await _factory.CreateAuthenticatedClientAsync();
        using var uploadContent = new MultipartFormDataContent();
        uploadContent.Add(new StringContent("Fatima Begum"), "PatientName");
        uploadContent.Add(new StringContent("32"), "PatientAge");
        var dummyBytes = Encoding.UTF8.GetBytes("Fake Prescription Image Bytes");
        var byteContent = new ByteArrayContent(dummyBytes);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        uploadContent.Add(byteContent, "files", "prescription.jpg");

        var uploadResp = await customerClient.PostAsync("/api/v1/prescriptions", uploadContent);
        uploadResp.EnsureSuccessStatusCode();
        var uploadJson = await uploadResp.Content.ReadAsStringAsync();
        using var uploadDoc = JsonDocument.Parse(uploadJson);
        var presId = uploadDoc.RootElement.GetProperty("data").GetProperty("id").GetInt64();

        // Arrange 2: Pharmacist logs in
        var pharmacistClient = await _factory.CreateAuthenticatedClientAsync(role: RoleNames.Pharmacist);

        // Act: Pharmacist reviews prescription
        var reviewReq = new ReviewPrescriptionRequest(
            NewStatus: PrescriptionStatuses.Approved,
            DecisionNotes: "Prescription verified against BMDC registration.",
            RejectionReason: null,
            VerifiedMedicinesJson: "[]"
        );

        var reviewContent = new StringContent(JsonSerializer.Serialize(reviewReq), Encoding.UTF8, "application/json");
        var reviewResp = await pharmacistClient.PostAsync($"/api/v1/prescriptions/{presId}/review", reviewContent);

        // Assert: 200 OK and status is approved
        Assert.Equal(HttpStatusCode.OK, reviewResp.StatusCode);
        var reviewJson = await reviewResp.Content.ReadAsStringAsync();
        using var reviewDoc = JsonDocument.Parse(reviewJson);
        var status = reviewDoc.RootElement.GetProperty("data").GetProperty("status").GetString();
        Assert.Equal(PrescriptionStatuses.Approved, status);
    }

    [Fact]
    public async Task CheckoutWorkflow_CompletesFullOrderCycle_AndPayment()
    {
        // Arrange: Authenticated customer
        var client = await _factory.CreateAuthenticatedClientAsync();

        // 1. Get an active OTC product
        var prodResp = await client.GetAsync("/api/v1/products?isPrescriptionRequired=false&pageSize=1");
        var prodJson = await prodResp.Content.ReadAsStringAsync();
        using var prodDoc = JsonDocument.Parse(prodJson);
        var product = prodDoc.RootElement.GetProperty("items")[0];
        var productId = product.GetProperty("id").GetInt64();

        // 2. Add product to cart
        var addReq = new AddToCartRequest(ProductId: productId, Quantity: 1);
        var addContent = new StringContent(JsonSerializer.Serialize(addReq), Encoding.UTF8, "application/json");
        var addResp = await client.PostAsync("/api/v1/cart/items", addContent);
        Assert.Equal(HttpStatusCode.OK, addResp.StatusCode);

        // 3. Checkout with NewAddress, Standard Delivery (ID 1), COD (ID 1)
        var checkoutReq = new CheckoutRequest(
            DeliveryAddressId: null,
            NewAddress: new OrderAddressInput(
                RecipientName: "Tariqul Islam",
                RecipientPhone: "01712998877",
                Division: "Dhaka",
                District: "Dhaka",
                Upazila: "Dhanmondi",
                AreaDetails: "House 12, Road 5, Dhanmondi, Dhaka",
                PostalCode: "1205",
                DeliveryInstructions: "Call before arrival"
            ),
            DeliveryMethodId: 1, // Standard Delivery
            PaymentMethodId: 1,  // Cash on Delivery
            PrescriptionId: null,
            CouponCode: null,
            CustomerNotes: "Urgent medicine delivery",
            IdempotencyKey: Guid.NewGuid().ToString()
        );

        var checkoutContent = new StringContent(JsonSerializer.Serialize(checkoutReq), Encoding.UTF8, "application/json");
        var checkoutResp = await client.PostAsync("/api/v1/orders/checkout", checkoutContent);

        // Assert: Order placed
        Assert.Equal(HttpStatusCode.OK, checkoutResp.StatusCode);
        var orderJson = await checkoutResp.Content.ReadAsStringAsync();
        using var orderDoc = JsonDocument.Parse(orderJson);
        var orderData = orderDoc.RootElement.GetProperty("data");

        var orderId = orderData.GetProperty("id").GetInt64();
        var orderNumber = orderData.GetProperty("orderNumber").GetString();
        Assert.StartsWith("ORD-", orderNumber);
        Assert.Equal(OrderStatuses.Pending, orderData.GetProperty("orderStatus").GetString());

        // 4. Initiate payment
        var initReq = new InitiatePaymentRequest(orderId);
        var initContent = new StringContent(JsonSerializer.Serialize(initReq), Encoding.UTF8, "application/json");
        var initResp = await client.PostAsync("/api/v1/payments/initiate", initContent);
        Assert.Equal(HttpStatusCode.OK, initResp.StatusCode);
        var initJson = await initResp.Content.ReadAsStringAsync();
        using var initDoc = JsonDocument.Parse(initJson);
        var paymentPublicId = initDoc.RootElement.GetProperty("data").GetProperty("paymentPublicId").GetGuid();

        // 5. Execute payment
        var execReq = new ExecutePaymentRequest(paymentPublicId, null, null);
        var execContent = new StringContent(JsonSerializer.Serialize(execReq), Encoding.UTF8, "application/json");
        var execResp = await client.PostAsync("/api/v1/payments/execute", execContent);
        Assert.Equal(HttpStatusCode.OK, execResp.StatusCode);

        // 6. Verify order status advanced to confirmed
        var getOrderResp = await client.GetAsync($"/api/v1/orders/{orderId}");
        var getOrderJson = await getOrderResp.Content.ReadAsStringAsync();
        using var getOrderDoc = JsonDocument.Parse(getOrderJson);
        var updatedOrder = getOrderDoc.RootElement.GetProperty("data");
        Assert.Equal(OrderStatuses.Confirmed, updatedOrder.GetProperty("orderStatus").GetString());
        Assert.Equal(PaymentStatuses.Paid, updatedOrder.GetProperty("paymentStatus").GetString());
    }
}
