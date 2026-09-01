using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Domain.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;

namespace HAMSA.Application.Features.Expenses.Queries;

public record GetExpensePredictionQuery(Guid BuildingId, Guid ManagerUserId);

public record ExpensePredictionResult(
    bool Success,
    string Message,
    decimal? PredictedAmount = null,
    decimal? ConfidenceScore = null,
    string? Trend = null,
    string? Analysis = null,
    List<string>? Recommendations = null);

public class GetExpensePredictionHandler
{
    private readonly IBuildingExpenseRepository _expenseRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IExpensePredictionAIService _predictionService;

    private static readonly string[] PersianMonthNames =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

    public GetExpensePredictionHandler(
        IBuildingExpenseRepository expenseRepository,
        IBuildingMembershipRepository membershipRepository,
        IBuildingRepository buildingRepository,
        IExpensePredictionAIService predictionService)
    {
        _expenseRepository = expenseRepository;
        _membershipRepository = membershipRepository;
        _buildingRepository = buildingRepository;
        _predictionService = predictionService;
    }

    public async Task<ExpensePredictionResult> HandleAsync(GetExpensePredictionQuery query)
    {
        var user = await _membershipRepository.GetCoMembersByUserInBuildingAsync(query.ManagerUserId, query.BuildingId);
        if(user.IsNullOrEmpty())
            return new ExpensePredictionResult(false, "شما عضو این ساختمان نیستید");
        
        var building = await _buildingRepository.GetByIdAsync(query.BuildingId);
        if (building is null)
            return new(false, "ساختمان یافت نشد.");

        // هزینه‌های ثابت ماهانه (از Building)
        var fixedMonthlyCosts = building.SharedElectricityCost
                              + building.SharedWaterCost
                              + building.CleaningCost
                              + building.ElevatorCost;

        // جمع‌آوری داده‌های ۶ ماه اخیر
        var now = DateTime.UtcNow;
        var historicalData = new List<HistoricalMonthData>();

        for (int i = 5; i >= 0; i--)
        {
            var targetDate = now.AddMonths(-i);
            var year = targetDate.Year;
            var month = targetDate.Month;

            var variableCosts = await _expenseRepository.GetTotalExpensesAsync(query.BuildingId, year, month);

            historicalData.Add(new HistoricalMonthData(
                year,
                month,
                PersianMonthNames[month - 1],
                fixedMonthlyCosts,
                variableCosts,
                fixedMonthlyCosts + variableCosts));
        }

        // اگر همه ماه‌ها صفر بودند، پیش‌بینی بی‌معنی است
        if (historicalData.All(h => h.TotalCosts == 0))
            return new(false, "داده کافی برای پیش‌بینی وجود ندارد. لطفاً ابتدا هزینه‌های چند ماه را ثبت کنید.");

        try
        {
            var request = new ExpensePredictionRequest(
                query.BuildingId,
                building.Name,
                building.UnitCount,
                historicalData);

            var prediction = await _predictionService.PredictNextMonthAsync(request);

            return new(
                true,
                "پیش‌بینی هزینه ماه آینده با موفقیت انجام شد.",
                prediction.PredictedNextMonth,
                prediction.ConfidenceScore,
                prediction.TrendDirection,
                prediction.Analysis,
                prediction.Recommendations);
        }
        catch (Exception ex)
        {
            return new(false, $"خطا در ارتباط با سرویس هوش مصنوعی: {ex.Message}");
        }
    }
}