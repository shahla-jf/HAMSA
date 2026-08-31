using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Charges.Commands.VerifyPayment;

public record VerifyPaymentCommand(
    Guid UserId,
    Guid TransactionId, 
    bool IsApproved);

public record VerifyPaymentResult(bool Success, string Message);

public class VerifyPaymentHandler
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IChargeRepository _chargeRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public VerifyPaymentHandler(
        ITransactionRepository transactionRepository,
        IChargeRepository chargeRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _transactionRepository = transactionRepository;
        _chargeRepository = chargeRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<VerifyPaymentResult> HandleAsync(VerifyPaymentCommand command)
    {
        // ۱. دریافت تراکنش
        var transaction = await _transactionRepository.GetByIdAsync(command.TransactionId);
        if (transaction is null)
            return new VerifyPaymentResult(false, "تراکنش یافت نشد");

        // ۲. دریافت شارژ مرتبط (برای جلوگیری از خطای Null Reference و دسترسی به BuildingId)
        var charge = await _chargeRepository.GetByIdAsync(transaction.ChargeId);
        if (charge is null)
            return new VerifyPaymentResult(false, "شارژ مرتبط یافت نشد");

        // ۳. بررسی دسترسی مدیر (حالا charge لود شده و BuildingId در دسترس است)
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(charge.BuildingId);
        if (managerId != command.UserId)
            return new VerifyPaymentResult(false, "فقط مدیر ساختمان می‌تواند این پرداخت را تایید کند");
        
        // ۴. بررسی وضعیت تراکنش
        if (transaction.Status != TransactionStatus.PendingVerification)
             return new VerifyPaymentResult(false, "این تراکنش قبلاً بررسی و تعیین تکلیف شده است");

        // ۵. بررسی وضعیت شارژ
        if (charge.IsPaid)
            return new VerifyPaymentResult(false, "این شارژ قبلاً پرداخت شده است");

        // ۶. اعمال تغییرات
        if (command.IsApproved)
        {
            transaction.MarkAsPaid(transaction.TrackingCode ?? "Manual-Verified");
            _transactionRepository.Update(transaction);

            charge.MarkAsPaid();
            _chargeRepository.Update(charge);
        }
        else
        {
            transaction.MarkAsFailed();
            _transactionRepository.Update(transaction);
        }
        
        await _transactionRepository.SaveChangesAsync();

        var successMessage = command.IsApproved 
            ? "پرداخت با موفقیت تایید و شارژ تسویه شد." 
            : "پرداخت رد شد. کاربر می‌تواند با کد پیگیری صحیح مجدداً اقدام کند.";

        return new VerifyPaymentResult(true, successMessage);
    }
}