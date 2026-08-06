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
    private readonly IChargeRepository _chargeRepository;
    private readonly IUnitRepository _unitRepository;

    public SetMonthlyChargeAmountHandler(
        IBuildingMembershipRepository membershipRepository,
        IMonthlyChargeRateRepository rateRepository,
        IChargeRepository chargeRepository,
        IUnitRepository unitRepository)
    {
        _membershipRepository = membershipRepository;
        _rateRepository = rateRepository;
        _chargeRepository = chargeRepository;
        _unitRepository = unitRepository;
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
            
            var units = await _unitRepository.GetByBuildingIdAsync(command.BuildingId);
            var dueDate = new DateTime(command.Year, command.Month, 1).AddMonths(1).AddDays(-1); // آخر همون ماه

            int count = 0;
            foreach (var unit in units)
            {
                var existingCharge = await _chargeRepository.GetByUnitAndMonthAsync(unit.Id, command.Year, command.Month);
                if (existingCharge is not null) continue; // قبلاً صادر شده

                var charge = Charge.Create(unit.Id, command.BuildingId, command.Year, command.Month, rate.Amount, dueDate);
                await _chargeRepository.AddAsync(charge);
                count++;
            }
            
            await _chargeRepository.SaveChangesAsync();

            rate.MarkAsIssued();
            _rateRepository.Update(rate);
            await _rateRepository.SaveChangesAsync();

            return new SetMonthlyChargeAmountResult(true, $"شارژ برای {count} واحد صادر شد");
        }
        else
        {
            return new SetMonthlyChargeAmountResult(false, "شارژ این ماه قبلاً صادر شده و قابل ویرایش نیست");
        }
    }
}
