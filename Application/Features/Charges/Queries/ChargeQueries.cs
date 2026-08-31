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
    bool IsPaid, DateTime DueDate);

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
            charge.IsPaid, charge.DueDate);
    }
}

// ======================================================
// GetUnpaidCharges — برای مدیر: لیست شارژهای پرداخت‌شده
// ======================================================
public record GetPaidChargesQuery(Guid BuildingId, Guid RequestingManagerId);

public record PaidChargeItem(
    Guid ChargeId, int Block, int Floor, int UnitNumber,
    int Year, int Month, decimal Amount, string? TrackingCode);

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
                c.Id, 
                c.Unit.Block, 
                c.Unit.Floor, 
                c.Unit.UnitNumber,
                c.Year, 
                c.Month, 
                c.Amount,
                c.Transactions.FirstOrDefault(t => t.Status == TransactionStatus.Paid)?.TrackingCode
            ))
            .Where(c => c.Year == DateTime.Now.Year && c.Month == DateTime.Now.Month)
            .ToList();
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


public record GetUnitPaymentStatusQuery(
    Guid UnitId,
    Guid ManagerUserId);

public record GetUnitPaymentStatusResult(
    bool Success,
    string Message,
    PaymentStatusEnum Status,
    
    // این فیلدها فقط وقتی پر می‌شوند که وضعیت PendingVerification باشد
    Guid? TransactionId = null,
    string? TrackingCode = null,
    decimal? Amount = null,
    DateTime? RequestDate = null);
    
    public class GetUnitPaymentStatusHandler
{
    private readonly IUnitRepository _unitRepository; 
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IChargeRepository _chargeRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetUnitPaymentStatusHandler(
        IUnitRepository unitRepository,
        IBuildingMembershipRepository membershipRepository,
        IChargeRepository chargeRepository,
        ITransactionRepository transactionRepository)
    {
        _unitRepository = unitRepository;
        _membershipRepository = membershipRepository;
        _chargeRepository = chargeRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<GetUnitPaymentStatusResult> HandleAsync(GetUnitPaymentStatusQuery query)
    {
        // ۱. دریافت واحد برای پیدا کردن BuildingId
        var unit = await _unitRepository.GetByIdAsync(query.UnitId);
        if (unit is null)
            return new GetUnitPaymentStatusResult(false, "واحد مورد نظر یافت نشد", PaymentStatusEnum.NotPaid);

        // ۲. بررسی دسترسی مدیر ساختمان
        var currentManagerId = await _membershipRepository.GetCurrentManagerIdAsync(unit.BuildingId);
        if (currentManagerId != query.ManagerUserId)
            return new GetUnitPaymentStatusResult(false, "شما دسترسی مدیریت این ساختمان را ندارید", PaymentStatusEnum.NotPaid);

        // ۳. دریافت لیست شارژهای واحد (که توسط ریپازیتوری شما از قبل بر اساس سال و ماه مرتب شده است)
        var charges = await _chargeRepository.GetByUnitIdAsync(query.UnitId);
        
        // استفاده از FirstOrDefault استاندارد LINQ روی IEnumerable (کاملاً امن و بهینه)
        var latestCharge = charges.FirstOrDefault();

        if (latestCharge is null)
            return new GetUnitPaymentStatusResult(true, "هیچ صورتحسابی برای این واحد ثبت نشده است", PaymentStatusEnum.NotPaid);

        // ۴. اگر شارژ قبلاً پرداخت شده باشد
        if (latestCharge.IsPaid)
        {
            return new GetUnitPaymentStatusResult(
                true,
                "این شارژ قبلاً پرداخت و تسویه شده است",
                PaymentStatusEnum.Paid,
                Amount: latestCharge.Amount
            );
        }

        // ۵. بررسی وجود تراکنش در انتظار تایید برای این شارژ خاص
        var pendingTransaction = await _transactionRepository.GetPendingByChargeIdAsync(latestCharge.Id);

        if (pendingTransaction is not null)
        {
            // وضعیت: در انتظار تایید (فرانت‌اند TransactionId را برای دکمه تایید/رد استفاده می‌کند)
            return new GetUnitPaymentStatusResult(
                true,
                "درخواست پرداخت با کد پیگیری ثبت شده و در انتظار تایید شماست",
                PaymentStatusEnum.PendingVerification,
                TransactionId: pendingTransaction.Id,
                TrackingCode: pendingTransaction.TrackingCode,
                Amount: pendingTransaction.Amount,
                RequestDate: pendingTransaction.CreatedAt
            );
        }

        // ۶. اگر نه پرداخت شده و نه تراکنش در انتظاری دارد
        return new GetUnitPaymentStatusResult(
            true,
            "هنوز پرداختی برای این صورتحساب ثبت نشده است",
            PaymentStatusEnum.NotPaid
        );
    }
}