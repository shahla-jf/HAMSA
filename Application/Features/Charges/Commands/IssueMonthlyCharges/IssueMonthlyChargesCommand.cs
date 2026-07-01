using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Charges.Commands.IssueMonthlyCharges;

// ------- Command -------
// مدیر این رو می‌زنه تا شارژ ماه جاری برای همه واحدها صادر بشه
// (یا می‌تونه با یه Scheduled Job خودکار هر اول ماه اجرا بشه)
public record IssueMonthlyChargesCommand(Guid BuildingId, Guid RequestingManagerId, int Year, int Month);

// ------- Result -------
public record IssueMonthlyChargesResult(bool Success, string Message, int UnitsCharged = 0);

// ------- Handler -------
public class IssueMonthlyChargesHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IMonthlyChargeRateRepository _rateRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IChargeRepository _chargeRepository;

    public IssueMonthlyChargesHandler(
        IBuildingMembershipRepository membershipRepository,
        IMonthlyChargeRateRepository rateRepository,
        IUnitRepository unitRepository,
        IChargeRepository chargeRepository)
    {
        _membershipRepository = membershipRepository;
        _rateRepository = rateRepository;
        _unitRepository = unitRepository;
        _chargeRepository = chargeRepository;
    }

    public async Task<IssueMonthlyChargesResult> HandleAsync(IssueMonthlyChargesCommand command)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.RequestingManagerId)
            return new IssueMonthlyChargesResult(false, "فقط مدیر ساختمان می‌تواند شارژ صادر کند");

        var rate = await _rateRepository.GetByMonthAsync(command.BuildingId, command.Year, command.Month);
        if (rate is null)
            return new IssueMonthlyChargesResult(false, "ابتدا مبلغ شارژ این ماه را تعیین کنید");

        if (rate.IsIssued)
            return new IssueMonthlyChargesResult(false, "شارژ این ماه قبلاً صادر شده است");

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

        return new IssueMonthlyChargesResult(true, $"شارژ برای {count} واحد صادر شد", count);
    }
}
