using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Domain.Interfaces.Services;

namespace HAMSA.Application.Features.Charges.Commands.RequestPayment;

// ------- Command -------
public record RequestPaymentCommand(Guid ChargeId, Guid RequestingUserId, string CallbackBaseUrl);

// ------- Result -------
public record RequestPaymentResult(bool Success, string Message, string? PaymentUrl = null);

// ------- Handler -------
public class RequestPaymentHandler
{
    private readonly IChargeRepository _chargeRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentGateway _paymentGateway;

    public RequestPaymentHandler(
        IChargeRepository chargeRepository,
        ITransactionRepository transactionRepository,
        IPaymentGateway paymentGateway)
    {
        _chargeRepository = chargeRepository;
        _transactionRepository = transactionRepository;
        _paymentGateway = paymentGateway;
    }

    public async Task<RequestPaymentResult> HandleAsync(RequestPaymentCommand command)
    {
        var charge = await _chargeRepository.GetByIdAsync(command.ChargeId);
        if (charge is null)
            return new RequestPaymentResult(false, "شارژ یافت نشد");

        if (charge.IsPaid)
            return new RequestPaymentResult(false, "این شارژ قبلاً پرداخت شده است");

        var totalAmount = charge.Amount + charge.PenaltyAmount;

        // تراکنش pending بساز
        var transaction = Transaction.Create(command.RequestingUserId, charge.Id, totalAmount);
        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        // درخواست پرداخت به درگاه
        var callbackUrl = $"{command.CallbackBaseUrl}/api/charge/verify-payment?transactionId={transaction.Id}";
        var paymentResult = await _paymentGateway.RequestPaymentAsync(
            totalAmount, $"پرداخت شارژ - {charge.Month}/{charge.Year}", callbackUrl);

        if (!paymentResult.Success)
        {
            transaction.MarkAsFailed();
            _transactionRepository.Update(transaction);
            await _transactionRepository.SaveChangesAsync();
            return new RequestPaymentResult(false, "خطا در اتصال به درگاه پرداخت");
        }

        return new RequestPaymentResult(true, "در حال انتقال به درگاه پرداخت", paymentResult.PaymentUrl);
    }
}
