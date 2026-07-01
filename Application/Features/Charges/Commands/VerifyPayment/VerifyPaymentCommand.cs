using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Domain.Interfaces.Services;

namespace HAMSA.Application.Features.Charges.Commands.VerifyPayment;

// ------- Command -------
public record VerifyPaymentCommand(Guid TransactionId, string Authority);

// ------- Result -------
public record VerifyPaymentResult(bool Success, string Message, string? TrackingCode = null);

// ------- Handler -------
public class VerifyPaymentHandler
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IChargeRepository _chargeRepository;
    private readonly IPaymentGateway _paymentGateway;

    public VerifyPaymentHandler(
        ITransactionRepository transactionRepository,
        IChargeRepository chargeRepository,
        IPaymentGateway paymentGateway)
    {
        _transactionRepository = transactionRepository;
        _chargeRepository = chargeRepository;
        _paymentGateway = paymentGateway;
    }

    public async Task<VerifyPaymentResult> HandleAsync(VerifyPaymentCommand command)
    {
        var transaction = await _transactionRepository.GetByIdAsync(command.TransactionId);
        if (transaction is null)
            return new VerifyPaymentResult(false, "تراکنش یافت نشد");

        var verifyResult = await _paymentGateway.VerifyPaymentAsync(command.Authority, transaction.Amount);

        if (!verifyResult.Success)
        {
            transaction.MarkAsFailed();
            _transactionRepository.Update(transaction);
            await _transactionRepository.SaveChangesAsync();
            return new VerifyPaymentResult(false, "پرداخت ناموفق بود");
        }

        transaction.MarkAsPaid(verifyResult.TrackingCode!);
        _transactionRepository.Update(transaction);
        await _transactionRepository.SaveChangesAsync();

        var charge = await _chargeRepository.GetByIdAsync(transaction.ChargeId);
        if (charge is not null && !charge.IsPaid)
        {
            charge.MarkAsPaid();
            _chargeRepository.Update(charge);
            await _chargeRepository.SaveChangesAsync();
        }

        return new VerifyPaymentResult(true, "پرداخت با موفقیت انجام شد", verifyResult.TrackingCode);
    }
}
