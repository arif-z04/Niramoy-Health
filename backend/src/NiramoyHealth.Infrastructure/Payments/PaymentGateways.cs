using System.Text.Json;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Entities;

namespace NiramoyHealth.Infrastructure.Payments;

public class BkashPaymentGateway : IPaymentGateway
{
    public string ProviderCode => "bkash";

    public Task<PaymentGatewayInitResult> InitializePaymentAsync(Payment payment, Order order, CancellationToken ct = default)
    {
        var gatewayPaymentId = $"BKASH_{Guid.NewGuid():N}";
        var redirectUrl = $"https://sandbox.checkout.bkash.com/payment/process?paymentID={gatewayPaymentId}&orderID={order.OrderNumber}&amount={payment.Amount:F2}";

        var rawResponse = JsonSerializer.Serialize(new
        {
            statusCode = "0000",
            statusMessage = "Successful",
            paymentID = gatewayPaymentId,
            bkashURL = redirectUrl,
            amount = payment.Amount.ToString("F2"),
            currency = payment.Currency,
            intent = "sale",
            merchantInvoiceNumber = order.OrderNumber
        });

        return Task.FromResult(new PaymentGatewayInitResult(
            Success: true,
            GatewayPaymentId: gatewayPaymentId,
            RedirectUrl: redirectUrl,
            ErrorMessage: null,
            RawResponseJson: rawResponse
        ));
    }

    public Task<PaymentGatewayExecutionResult> ExecutePaymentAsync(Payment payment, string? gatewayPaymentId, string? trxId, CancellationToken ct = default)
    {
        var transactionId = trxId ?? $"BKS{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        var rawResponse = JsonSerializer.Serialize(new
        {
            statusCode = "0000",
            statusMessage = "Successful",
            paymentID = gatewayPaymentId ?? payment.TransactionId,
            trxID = transactionId,
            amount = payment.Amount.ToString("F2"),
            transactionStatus = "Completed",
            paymentExecuteTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        });

        return Task.FromResult(new PaymentGatewayExecutionResult(
            Success: true,
            TransactionId: transactionId,
            ErrorMessage: null,
            RawResponseJson: rawResponse
        ));
    }

    public Task<PaymentGatewayRefundResult> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken ct = default)
    {
        var refundTrxId = $"BKS_REF_{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

        var rawResponse = JsonSerializer.Serialize(new
        {
            statusCode = "0000",
            statusMessage = "Successful",
            refundTrxID = refundTrxId,
            originalTrxID = payment.TransactionId,
            amount = amount.ToString("F2"),
            reason = reason
        });

        return Task.FromResult(new PaymentGatewayRefundResult(
            Success: true,
            RefundTransactionId: refundTrxId,
            ErrorMessage: null,
            RawResponseJson: rawResponse
        ));
    }
}

public class NagadPaymentGateway : IPaymentGateway
{
    public string ProviderCode => "nagad";

    public Task<PaymentGatewayInitResult> InitializePaymentAsync(Payment payment, Order order, CancellationToken ct = default)
    {
        var gatewayPaymentId = $"NAGAD_{Guid.NewGuid():N}";
        var redirectUrl = $"https://sandbox.engine.nagad.com.bd/check-out/index.html?paymentID={gatewayPaymentId}&orderID={order.OrderNumber}&amount={payment.Amount:F2}";

        var rawResponse = JsonSerializer.Serialize(new
        {
            status = "Success",
            paymentReferenceId = gatewayPaymentId,
            callBackUrl = redirectUrl,
            orderId = order.OrderNumber,
            amount = payment.Amount.ToString("F2")
        });

        return Task.FromResult(new PaymentGatewayInitResult(
            Success: true,
            GatewayPaymentId: gatewayPaymentId,
            RedirectUrl: redirectUrl,
            ErrorMessage: null,
            RawResponseJson: rawResponse
        ));
    }

    public Task<PaymentGatewayExecutionResult> ExecutePaymentAsync(Payment payment, string? gatewayPaymentId, string? trxId, CancellationToken ct = default)
    {
        var transactionId = trxId ?? $"NGD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        var rawResponse = JsonSerializer.Serialize(new
        {
            status = "Success",
            paymentRefId = gatewayPaymentId ?? payment.TransactionId,
            issuerPaymentRefNo = transactionId,
            amount = payment.Amount.ToString("F2"),
            paymentExecuteTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        });

        return Task.FromResult(new PaymentGatewayExecutionResult(
            Success: true,
            TransactionId: transactionId,
            ErrorMessage: null,
            RawResponseJson: rawResponse
        ));
    }

    public Task<PaymentGatewayRefundResult> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken ct = default)
    {
        var refundTrxId = $"NGD_REF_{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

        var rawResponse = JsonSerializer.Serialize(new
        {
            status = "Success",
            refundReferenceId = refundTrxId,
            originalTrxId = payment.TransactionId,
            amount = amount.ToString("F2")
        });

        return Task.FromResult(new PaymentGatewayRefundResult(
            Success: true,
            RefundTransactionId: refundTrxId,
            ErrorMessage: null,
            RawResponseJson: rawResponse
        ));
    }
}

public class CodPaymentGateway : IPaymentGateway
{
    public string ProviderCode => "cod";

    public Task<PaymentGatewayInitResult> InitializePaymentAsync(Payment payment, Order order, CancellationToken ct = default)
    {
        var rawResponse = JsonSerializer.Serialize(new
        {
            method = "cash_on_delivery",
            orderId = order.OrderNumber,
            amount = payment.Amount.ToString("F2"),
            note = "Payment will be collected in cash upon package delivery."
        });

        return Task.FromResult(new PaymentGatewayInitResult(
            Success: true,
            GatewayPaymentId: $"COD_{order.OrderNumber}",
            RedirectUrl: null,
            ErrorMessage: null,
            RawResponseJson: rawResponse
        ));
    }

    public Task<PaymentGatewayExecutionResult> ExecutePaymentAsync(Payment payment, string? gatewayPaymentId, string? trxId, CancellationToken ct = default)
    {
        var rawResponse = JsonSerializer.Serialize(new
        {
            status = "CashCollected",
            collectedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        });

        return Task.FromResult(new PaymentGatewayExecutionResult(
            Success: true,
            TransactionId: $"COD_TRX_{DateTime.UtcNow:yyyyMMddHHmmss}",
            ErrorMessage: null,
            RawResponseJson: rawResponse
        ));
    }

    public Task<PaymentGatewayRefundResult> ProcessRefundAsync(Payment payment, decimal amount, string reason, CancellationToken ct = default)
    {
        var rawResponse = JsonSerializer.Serialize(new
        {
            status = "CashRefundRecorded",
            amount = amount.ToString("F2"),
            reason = reason
        });

        return Task.FromResult(new PaymentGatewayRefundResult(
            Success: true,
            RefundTransactionId: $"COD_REF_{DateTime.UtcNow:yyyyMMddHHmmss}",
            ErrorMessage: null,
            RawResponseJson: rawResponse
        ));
    }
}
