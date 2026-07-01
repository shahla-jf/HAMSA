namespace HAMSA.Domain.Interfaces.Services;

public record PaymentRequestResult(bool Success, string? PaymentUrl, string? Authority);
public record PaymentVerifyResult(bool Success, string? TrackingCode);

public interface IPaymentGateway
{
    Task<PaymentRequestResult> RequestPaymentAsync(decimal amount, string description, string callbackUrl);
    Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, decimal amount);
}
