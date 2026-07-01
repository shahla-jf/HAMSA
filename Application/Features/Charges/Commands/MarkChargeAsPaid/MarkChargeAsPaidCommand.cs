using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Charges.Commands.MarkChargeAsPaid;

// ------- Command -------
// مدیر دستی شارژ رو پرداخت‌شده علامت می‌زنه (مثلاً پرداخت نقدی)
public record MarkChargeAsPaidCommand(Guid ChargeId, Guid RequestingManagerId);

// ------- Result -------
public record MarkChargeAsPaidResult(bool Success, string Message);

// ------- Handler -------
public class MarkChargeAsPaidHandler
{
    private readonly IChargeRepository _chargeRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public MarkChargeAsPaidHandler(
        IChargeRepository chargeRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _chargeRepository = chargeRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<MarkChargeAsPaidResult> HandleAsync(MarkChargeAsPaidCommand command)
    {
        var charge = await _chargeRepository.GetByIdAsync(command.ChargeId);
        if (charge is null)
            return new MarkChargeAsPaidResult(false, "شارژ یافت نشد");

        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(charge.BuildingId);
        if (managerId != command.RequestingManagerId)
            return new MarkChargeAsPaidResult(false, "فقط مدیر ساختمان می‌تواند این عملیات را انجام دهد");

        if (charge.IsPaid)
            return new MarkChargeAsPaidResult(false, "این شارژ قبلاً پرداخت شده است");

        charge.MarkAsPaid();
        _chargeRepository.Update(charge);
        await _chargeRepository.SaveChangesAsync();

        return new MarkChargeAsPaidResult(true, "شارژ با موفقیت پرداخت‌شده علامت زده شد");
    }
}
