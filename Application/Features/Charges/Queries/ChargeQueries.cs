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
// GetUnitChargeHistory — تاریخچه شارژهای یک واحد
// ======================================================
public record GetUnitChargeHistoryQuery(Guid UnitId);

public record ChargeHistoryItem(
    Guid ChargeId, int Year, int Month, decimal Amount,
    decimal PenaltyAmount, bool IsPaid, DateTime? PaidAt);

public class GetUnitChargeHistoryHandler
{
    private readonly IChargeRepository _chargeRepository;

    public GetUnitChargeHistoryHandler(IChargeRepository chargeRepository)
    {
        _chargeRepository = chargeRepository;
    }

    public async Task<IEnumerable<ChargeHistoryItem>> HandleAsync(GetUnitChargeHistoryQuery query)
    {
        var charges = await _chargeRepository.GetByUnitIdAsync(query.UnitId);
        return charges.Select(c => new ChargeHistoryItem(
            c.Id, c.Year, c.Month, c.Amount, c.PenaltyAmount, c.IsPaid, c.PaidAt));
    }
}

// ======================================================
// GetUnpaidCharges — برای مدیر: لیست شارژهای پرداخت‌نشده
// ======================================================
public record GetUnpaidChargesQuery(Guid BuildingId, Guid RequestingManagerId);

public record UnpaidChargeItem(
    Guid ChargeId, int Block, int Floor, int UnitNumber,
    int Year, int Month, decimal Amount, decimal PenaltyAmount, DateTime DueDate);

public class GetUnpaidChargesHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IChargeRepository _chargeRepository;

    public GetUnpaidChargesHandler(
        IBuildingMembershipRepository membershipRepository,
        IChargeRepository chargeRepository)
    {
        _membershipRepository = membershipRepository;
        _chargeRepository = chargeRepository;
    }

    public async Task<IEnumerable<UnpaidChargeItem>> HandleAsync(GetUnpaidChargesQuery query)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        if (managerId != query.RequestingManagerId)
            return Enumerable.Empty<UnpaidChargeItem>();

        var charges = await _chargeRepository.GetUnpaidByBuildingAsync(query.BuildingId);

        return charges.Select(c => new UnpaidChargeItem(
            c.Id, c.Unit.Block, c.Unit.Floor, c.Unit.UnitNumber,
            c.Year, c.Month, c.Amount, c.PenaltyAmount, c.DueDate));
    }
}

// ======================================================
// GetMyTransactions — تراکنش‌های کاربر (با فیلتر بازه زمانی اختیاری)
// ======================================================
public record GetMyTransactionsQuery(Guid UserId, DateTime? From = null, DateTime? To = null);

public record TransactionItem(
    Guid TransactionId, decimal Amount, TransactionStatus Status,
    string? TrackingCode, DateTime CreatedAt);

public class GetMyTransactionsHandler
{
    private readonly ITransactionRepository _transactionRepository;

    public GetMyTransactionsHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<IEnumerable<TransactionItem>> HandleAsync(GetMyTransactionsQuery query)
    {
        var transactions = query.From.HasValue && query.To.HasValue
            ? await _transactionRepository.GetByUserIdAndDateRangeAsync(query.UserId, query.From.Value, query.To.Value)
            : await _transactionRepository.GetByUserIdAsync(query.UserId);

        return transactions.Select(t => new TransactionItem(
            t.Id, t.Amount, t.Status, t.TrackingCode, t.CreatedAt));
    }
}
