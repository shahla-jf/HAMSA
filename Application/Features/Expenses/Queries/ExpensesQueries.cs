using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Expenses.Queries;

/// <summary>
/// دریافت لیست خریدهای وارد شده توسط مدیر
/// </summary>
public record GetBuildingExpensesQuery(
    Guid BuildingId,
    Guid ManagerUserId);

public record BuildingExpenseItem(
    Guid Id,
    string Category,
    string Title,
    decimal Amount,
    string CreatedBy,
    DateTime CreatedAt);

public class GetBuildingExpensesHandler
{
    private readonly IBuildingExpenseRepository _expenseRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetBuildingExpensesHandler(
        IBuildingExpenseRepository expenseRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _expenseRepository = expenseRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<BuildingExpenseItem>> HandleAsync(GetBuildingExpensesQuery query)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        
        if (managerId != query.ManagerUserId)
            return Enumerable.Empty<BuildingExpenseItem>();

        var expenses = await _expenseRepository.GetByBuildingIdAsync(query.BuildingId);

        return expenses.Select(e =>
            new BuildingExpenseItem(
                e.Id,
                e.Category.ToString(),
                e.Title,
                e.Amount,
                e.CreatedByUser != null ? $"{e.CreatedByUser.FirstName} {e.CreatedByUser.LastName}".Trim() : "نامشخص",
                e.CreatedAt));
    }
}



/// <summary>
/// کل هزینه‌های این ماه و پرخرج‌ترین هزینه
/// </summary>

// ------- Query -------
public record GetMonthlyExpenseSummaryQuery(
    Guid BuildingId,
    Guid ManagerUserId);

// ------- Result -------
public record MonthlyExpenseSummary(
    bool Success,
    string Message,
    decimal TotalExpenses = 0,
    string? HighestExpenseTitle = null);

// ------- Handler -------
public class GetMonthlyExpenseSummaryHandler
{
    private readonly IBuildingExpenseRepository _expenseRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IBuildingRepository _buildingRepository;

    public GetMonthlyExpenseSummaryHandler(
        IBuildingExpenseRepository expenseRepository,
        IBuildingMembershipRepository membershipRepository,
        IBuildingRepository buildingRepository)
    {
        _expenseRepository = expenseRepository;
        _membershipRepository = membershipRepository;
        _buildingRepository = buildingRepository;
    }

    public async Task<MonthlyExpenseSummary> HandleAsync(GetMonthlyExpenseSummaryQuery query)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        if (managerId != query.ManagerUserId)
            return new(false, "فقط مدیر ساختمان به گزارش مخارج دسترسی دارد.");

        
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        // ۱. دریافت هزینه‌های متفرقه ثبت شده توسط مدیر از جدول BuildingExpense
        var dbTotalExpenses = await _expenseRepository.GetTotalExpensesAsync(query.BuildingId, year, month);
        var dbExpenses = await _expenseRepository.GetByBuildingAndMonthAsync(query.BuildingId, year, month);

        // ۲. دریافت هزینه‌های ثابت مشاعات از Entity Building
        var building = await _buildingRepository.GetByIdAsync(query.BuildingId);
        
        if (building is null)
            return new(false, "ساختمان یافت نشد.");

        var sharedElectricity = building.SharedElectricityCost;
        var sharedWater = building.SharedWaterCost;
        var sharedCleaning = building.CleaningCost;
        var sharedElevator = building.ElevatorCost;

        // ۳. محاسبه مجموع کل هزینه‌ها
        var totalExpenses = dbTotalExpenses + sharedElectricity + sharedWater + sharedCleaning + sharedElevator;

        // ۴. پیدا کردن پرخرج‌ترین هزینه بین همه موارد
        var allExpenses = new List<(string Title, decimal Amount)>
        {
            ("هزینه برق مشاعات", sharedElectricity),
            ("هزینه آب مشاعات", sharedWater),
            ("هزینه نظافت", sharedCleaning),
            ("هزینه آسانسور", sharedElevator)
        };

        // اضافه کردن هزینه‌های دیتابیسی
        allExpenses.AddRange(dbExpenses.Select(e => (e.Title, e.Amount)));

        // پیدا کردن بیشترین مقدار
        var highestExpense = allExpenses
            .Where(x => x.Amount > 0) // نادیده گرفتن هزینه‌های صفر
            .OrderByDescending(x => x.Amount)
            .FirstOrDefault();

        var highestTitle = highestExpense.Title ?? "بدون هزینه";

        return new(
            true,
            "گزارش مخارج ماهانه با موفقیت دریافت شد.",
            totalExpenses,
            highestTitle);
    }
}



/// <summary>
/// نمودار ماهانه و سالانه
/// </summary>

// ------- Query -------
public record GetDashboardFinancialsQuery(
    Guid BuildingId,
    Guid ManagerUserId);

// ------- Result -------
public record MonthlyDataPoint(int Month, string MonthName, decimal Expenses);
public record YearlyDataPoint(int Year, decimal Expenses);

public record DashboardFinancials(
    bool Success,
    string Message,
    decimal CurrentMonthIncome = 0,
    decimal CurrentMonthExpenses = 0,
    List<MonthlyDataPoint>? MonthlyChartData = null,
    List<YearlyDataPoint>? YearlyChartData = null);

// ------- Handler -------
public class GetDashboardFinancialsHandler
{
    private readonly IBuildingExpenseRepository _expenseRepository;
    private readonly IChargeRepository _chargeRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IBuildingRepository _buildingRepository;

    public GetDashboardFinancialsHandler(
        IBuildingExpenseRepository expenseRepository,
        IChargeRepository chargeRepository,
        IBuildingMembershipRepository membershipRepository,
        IBuildingRepository buildingRepository)
    {
        _expenseRepository = expenseRepository;
        _chargeRepository = chargeRepository;
        _membershipRepository = membershipRepository;
        _buildingRepository = buildingRepository;
    }

    public async Task<DashboardFinancials> HandleAsync(GetDashboardFinancialsQuery query)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);

        if (managerId != query.ManagerUserId)
            return new DashboardFinancials(false, "شما مدیر این ساختمان نیستید");

        var now = DateTime.UtcNow;
        var currentYear = now.Year;
        var currentMonth = now.Month;

        // ۱. محاسبه درآمد این ماه (از شارژهای پرداخت شده)
        var currentMonthIncome = await _chargeRepository.GetMonthlyIncomeAsync(query.BuildingId, currentYear, currentMonth);

        // ۲. محاسبه هزینه‌های این ماه (BuildingExpense + هزینه‌های ثابت Building)
        var dbExpensesThisMonth = await _expenseRepository.GetTotalExpensesAsync(query.BuildingId, currentYear, currentMonth);
        
        var building = await _buildingRepository.GetByIdAsync(query.BuildingId);
        var sharedCosts = building is null ? 0 : 
            building.SharedElectricityCost + building.SharedWaterCost + building.CleaningCost + building.ElevatorCost;
        
        var currentMonthExpenses = dbExpensesThisMonth + sharedCosts;
        
        // ۳. داده‌های نمودار ماهانه (فقط تا ماه جاری، و حداکثر ۴ ماه اخیر)
        var monthlyExpensesDict = await _expenseRepository.GetMonthlyExpensesForYearAsync(query.BuildingId, currentYear);
        
        var monthlyChartData = new List<MonthlyDataPoint>();
        var monthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", 
                                 "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
        
        // حلقه فقط تا ماه جاری اجرا می‌شود تا از تولید داده‌های بی‌مورد برای ماه‌های آینده جلوگیری شود
        for (int month = 1; month <= currentMonth; month++)
        {
            var expenses = monthlyExpensesDict.GetValueOrDefault((currentYear, month), 0);
            
            // اگر ماه جاری است، هزینه‌های ثابت Building هم اضافه شود
            if (month == currentMonth)
                expenses += sharedCosts;
            
            monthlyChartData.Add(new MonthlyDataPoint(
                month, 
                monthNames[month - 1], 
                expenses));
        }

        // اگر تعداد ماه‌ها بیشتر از ۴ بود، فقط ۴ ماه اخیر را نگه دار
        if (monthlyChartData.Count > 4)
        {
            monthlyChartData = monthlyChartData.TakeLast(4).ToList();
        }

        // ۴. داده‌های نمودار سالانه (حداکثر ۴ سال آخر)
        var yearlyExpensesDict = await _expenseRepository.GetYearlyExpensesAsync(query.BuildingId);
        
        var yearlyChartData = yearlyExpensesDict.Keys
            .OrderBy(y => y)
            .TakeLast(4) // اگر بیشتر از ۴ سال بود، فقط ۴ سال آخر را برمی‌گرداند
            .Select(year => new YearlyDataPoint(
                year,
                yearlyExpensesDict.GetValueOrDefault(year, 0)
            )).ToList();

        return new(
            true,
            "اطلاعات مالی داشبورد با موفقیت دریافت شد.",
            currentMonthIncome,
            currentMonthExpenses,
            monthlyChartData,
            yearlyChartData);
    }
}