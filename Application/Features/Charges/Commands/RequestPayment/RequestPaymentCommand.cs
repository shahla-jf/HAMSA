using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Charges.Commands.RequestPayment;

// ------- Command -------
public record RequestPaymentCommand(
    Guid ChargeId, 
    Guid RequestingUserId, 
    string TrackingCode);

// ------- Result -------
public record RequestPaymentResult(bool Success, string Message);

// ------- Handler -------
public class RequestPaymentHandler
{
    private readonly IChargeRepository _chargeRepository;
    private readonly ITransactionRepository _transactionRepository;

    public RequestPaymentHandler(
        IChargeRepository chargeRepository,
        ITransactionRepository transactionRepository)
    {
        _chargeRepository = chargeRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<RequestPaymentResult> HandleAsync(RequestPaymentCommand command)
    {
        var charge = await _chargeRepository.GetByIdAsync(command.ChargeId);
        if (charge is null)
            return new RequestPaymentResult(false, "شارژ یافت نشد");

        if (charge.IsPaid)
            return new RequestPaymentResult(false, "این شارژ قبلاً پرداخت شده است");

        var hasPending = await _transactionRepository.ExistsAsync(t => t.ChargeId == command.ChargeId && t.Status == TransactionStatus.PendingVerification);
        if (hasPending)
             return new RequestPaymentResult(false, "یک درخواست پرداخت در انتظار تایید برای این شارژ وجود دارد");

        var totalAmount = charge.Amount + charge.PenaltyAmount;

        var transaction = Transaction.Create(command.RequestingUserId, charge.Id, totalAmount);
        
        // تنظیم وضعیت روی "در انتظار تایید" و ذخیره کد پیگیری
        transaction.SetAsPendingVerification(command.TrackingCode);

        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        return new RequestPaymentResult(true, "درخواست پرداخت شما ثبت شد و پس از بررسی مدیر، وضعیت شارژ به‌روزرسانی می‌شود.");
    }
}