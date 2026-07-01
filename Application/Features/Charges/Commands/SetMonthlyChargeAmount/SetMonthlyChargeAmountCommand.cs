using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Charges.Commands.SetMonthlyChargeAmount;

// ------- Command -------
public record SetMonthlyChargeAmountCommand(
    Guid BuildingId,
    Guid RequestingManagerId,
    int Year,
    int Month,
    decimal Amount
);

// ------- Result -------
public record SetMonthlyChargeAmountResult(bool Success, string Message);

// ------- Handler -------
public class SetMonthlyChargeAmountHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IMonthlyChargeRateRepository _rateRepository;

    public SetMonthlyChargeAmountHandler(
        IBuildingMembershipRepository membershipRepository,
        IMonthlyChargeRateRepository rateRepository)
    {
        _membershipRepository = membershipRepository;
        _rateRepository = rateRepository;
    }

    public async Task<SetMonthlyChargeAmountResult> HandleAsync(SetMonthlyChargeAmountCommand command)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.RequestingManagerId)
            return new SetMonthlyChargeAmountResult(false, "فقط مدیر ساختمان می‌تواند مبلغ شارژ را تعیین کند");

        var existing = await _rateRepository.GetByMonthAsync(command.BuildingId, command.Year, command.Month);

        if (existing is null)
        {
            var rate = MonthlyChargeRate.Create(command.BuildingId, command.Year, command.Month, command.Amount);
            await _rateRepository.AddAsync(rate);
        }
        else
        {
            if (existing.IsIssued)
                return new SetMonthlyChargeAmountResult(false, "شارژ این ماه قبلاً صادر شده و قابل ویرایش نیست");

            existing.UpdateAmount(command.Amount);
            _rateRepository.Update(existing);
        }

        await _rateRepository.SaveChangesAsync();
        return new SetMonthlyChargeAmountResult(true, "مبلغ شارژ با موفقیت ثبت شد");
    }
}
