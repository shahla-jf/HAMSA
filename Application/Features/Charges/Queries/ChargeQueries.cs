using System.Diagnostics.CodeAnalysis;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Charges.Queries;

// ======================================================
// GetChargeRates — صفحه مدیر: مبلغ شارژ ماه جاری و ماه آینده
// ======================================================
public record GetChargeRatesQuery(Guid BuildingId, Guid RequestingManagerId);

public record MonthRateInfo(
    int Year, int Month, string PersianMonthName,
    decimal? Amount, bool IsIssued, bool IsCurrentMonth);

public record GetChargeRatesResult(MonthRateInfo CurrentMonth, MonthRateInfo NextMonth);

public class GetChargeRatesHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IMonthlyChargeRateRepository _rateRepository;

    private static readonly string[] PersianMonths =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

    public GetChargeRatesHandler(
        IBuildingMembershipRepository membershipRepository,
        IMonthlyChargeRateRepository rateRepository)
    {
        _membershipRepository = membershipRepository;
        _rateRepository = rateRepository;
    }

    public async Task<GetChargeRatesResult?> HandleAsync(GetChargeRatesQuery query)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        if (managerId != query.RequestingManagerId)
            return null;

        var persianNow = new System.Globalization.PersianCalendar();
        var now = DateTime.UtcNow;
        int currentYear = persianNow.GetYear(now);
        int currentMonth = persianNow.GetMonth(now);

        int nextYear = currentMonth == 12 ? currentYear + 1 : currentYear;
        int nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;

        var currentRate = await _rateRepository.GetByMonthAsync(query.BuildingId, currentYear, currentMonth);
        var nextRate = await _rateRepository.GetByMonthAsync(query.BuildingId, nextYear, nextMonth);

        return new GetChargeRatesResult(
            new MonthRateInfo(currentYear, currentMonth, PersianMonths[currentMonth - 1],
                currentRate?.Amount, currentRate?.IsIssued ?? false, true),
            new MonthRateInfo(nextYear, nextMonth, PersianMonths[nextMonth - 1],
                nextRate?.Amount, nextRate?.IsIssued ?? false, false)
        );
    }
}

// ======================================================
// GetMyCurrentCharge — کاربر شارژ ماه جاری خودش رو می‌بینه
// ======================================================
public record GetMyCurrentChargeQuery(Guid UnitId);

public record MyChargeResult(
    Guid? ChargeId, int Year, int Month, decimal Amount,
    decimal PenaltyAmount, bool IsPaid, DateTime DueDate);

public class GetMyCurrentChargeHandler
{
    private readonly IChargeRepository _chargeRepository;

    public GetMyCurrentChargeHandler(IChargeRepository chargeRepository)
    {
        _chargeRepository = chargeRepository;
    }

    public async Task<MyChargeResult?> HandleAsync(GetMyCurrentChargeQuery query)
    {
        var persianNow = new System.Globalization.PersianCalendar();
        var now = DateTime.UtcNow;
        int year = persianNow.GetYear(now);
        int month = persianNow.GetMonth(now);

        var charge = await _chargeRepository.GetByUnitAndMonthAsync(query.UnitId, year, month);
        if (charge is null) return null;

        return new MyChargeResult(
            charge.Id, charge.Year, charge.Month, charge.Amount,
            charge.PenaltyAmount, charge.IsPaid, charge.DueDate);
    }
}

// ======================================================
// GetUnpaidCharges — برای مدیر: لیست شارژهای پرداخت‌شده
// ======================================================
public record GetPaidChargesQuery(Guid BuildingId, Guid RequestingManagerId);

public record PaidChargeItem(
    Guid ChargeId, int Block, int Floor, int UnitNumber,
    int Year, int Month, decimal Amount);

public class GetPaidChargesHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IChargeRepository _chargeRepository;

    public GetPaidChargesHandler(
        IBuildingMembershipRepository membershipRepository,
        IChargeRepository chargeRepository)
    {
        _membershipRepository = membershipRepository;
        _chargeRepository = chargeRepository;
    }

    public async Task<IEnumerable<PaidChargeItem>> HandleAsync(GetPaidChargesQuery query)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        if (managerId != query.RequestingManagerId)
            return Enumerable.Empty<PaidChargeItem>();

        var charges = await _chargeRepository.GetPaidByBuildingAsync(query.BuildingId);

        return charges.Select(c => new PaidChargeItem(
            c.Id, c.Unit.Block, c.Unit.Floor, c.Unit.UnitNumber,
            c.Year, c.Month, c.Amount))
            .Where(c => c.Year == DateTime.Now.Year && c.Month == DateTime.Now.Month);
    }
}


// ======================================================
// GetSharedCosts - مشاهده هزینه های مشاعات
// ======================================================
public record GetBuildingSharedCostsQuery(Guid BuildingId, Guid UserId);

public record SharedCosts(
    bool Success,
    string Message,
    Guid BuildingId,
    decimal Electricity,
    bool IsElectricityPaid,
    decimal Water,
    bool IsWaterPaid,
    decimal Cleaning,
    bool IsCleaningPaid,
    decimal Elevator,
    bool IsElevatorPaid
);

public class GetSharedCostsHandler
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _buildingMembershipRepository;
    
    public GetSharedCostsHandler(IBuildingRepository buildingRepository, IBuildingMembershipRepository buildingMembershipRepository)
    {
        _buildingRepository = buildingRepository;
        _buildingMembershipRepository = buildingMembershipRepository;
    }
    
    public async Task<SharedCosts> HandleAsync(GetBuildingSharedCostsQuery query)
    {
        var building = await _buildingRepository.GetByIdAsync(query.BuildingId);
        if (building == null)
            return new SharedCosts(false, "ساختمان یافت نشد", query.BuildingId, 0, false, 0, false, 0, false, 0, false);

        var user = await _buildingMembershipRepository.GetActiveAsync(query.UserId, query.BuildingId);
        if (user == null)
            return new SharedCosts(false, "شما عضو این ساختمان نیسیتید", query.BuildingId, 0, false, 0, false, 0, false, 0, false);
        
        return new SharedCosts(
            true,
            "مقادیر با موفقیت یافت شد",
            building.Id,
            building.SharedElectricityCost,
            building.IsElectricityPaid,
            building.SharedWaterCost,
            building.IsWaterPaid,
            building.CleaningCost,
            building.IsCleaningPaid,
            building.ElevatorCost,
            building.IsElevatorPaid);
    }
}