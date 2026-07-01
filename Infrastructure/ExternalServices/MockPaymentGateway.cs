using HAMSA.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace HAMSA.Infrastructure.ExternalServices;

// --- Payment Gateway (موقت - بعداً با N8N / زرین‌پال جایگزین میشه) ---
public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<PaymentRequestResult> RequestPaymentAsync(
        decimal amount, string description, string callbackUrl)
    {
        var fakeAuthority = Guid.NewGuid().ToString("N");
        _logger.LogInformation(
            "Mock payment requested: Amount={Amount}, Description={Description}, Authority={Authority}",
            amount, description, fakeAuthority);

        // در حالت واقعی اینجا URL درگاه پرداخت برمی‌گرده
        var fakePaymentUrl = $"{callbackUrl}?authority={fakeAuthority}&status=OK";

        return Task.FromResult(new PaymentRequestResult(true, fakePaymentUrl, fakeAuthority));
    }

    public Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, decimal amount)
    {
        // در حالت mock همیشه موفق فرض می‌کنیم
        var trackingCode = new Random().Next(10000000, 99999999).ToString();
        _logger.LogInformation(
            "Mock payment verified: Authority={Authority}, TrackingCode={TrackingCode}",
            authority, trackingCode);

        return Task.FromResult(new PaymentVerifyResult(true, trackingCode));
    }
}
