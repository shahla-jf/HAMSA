using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Expenses.Queries;

/// <summary>
/// دریافت لیست خریدهای وارد شده نوسط مدیر
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
        
        // فقط مدیر ساختمان می‌تواند لیست هزینه‌ها را ببیند
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
/// کل هزینه های این ماه و پرخرج ترین هزینه
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

    public GetMonthlyExpenseSummaryHandler(
        IBuildingExpenseRepository expenseRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _expenseRepository = expenseRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<MonthlyExpenseSummary> HandleAsync(GetMonthlyExpenseSummaryQuery query)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);

        // احراز هویت: فقط مدیر ساختمان دسترسی دارد
        if (managerId != query.ManagerUserId)
            return new(false, "فقط مدیر ساختمان به گزارش مخارج دسترسی دارد.");

        // استفاده از ماه و سال جاری در صورت عدم ارسال توسط کاربر
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        // فراخوانی متدهای موجود در Repository
        var totalExpenses = await _expenseRepository.GetTotalExpensesAsync(query.BuildingId, year, month);
        var (highestTitle, highestAmount) = await _expenseRepository.GetHighestExpenseAsync(query.BuildingId, year, month);

        return new(
            true,
            "گزارش مخارج ماهانه با موفقیت دریافت شد.",
            totalExpenses,
            highestTitle);
    }
}