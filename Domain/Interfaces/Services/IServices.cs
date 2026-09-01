using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Interfaces.Services;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string code);
}

public interface IJwtService
{
    string GenerateToken(Guid userId, string phoneNumber);
    Guid? ValidateToken(string token);
}

public interface IFileStorageService
{
    Task<string> UploadImageAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}

public interface IGroupChallengeAIService
{
    Task<AiChallengeResponse> GenerateChallengeAsync(
        Guid buildingId,
        List<ParticipantProfile> participants,
        CancellationToken cancellationToken = default);
}

public record ParticipantProfile(
    int Age,
    Gender Gender,
    string SportsBackground);

public record AiChallengeResponse(
    string Title,
    string Description);
    
    
    
    
public record HistoricalMonthData(
    int Year,
    int Month,
    string MonthName,
    decimal FixedCosts,     // آب + برق + نظافت + آسانسور
    decimal VariableCosts,  // هزینه‌های ثبت شده توسط مدیر
    decimal TotalCosts);

public record ExpensePredictionRequest(
    Guid BuildingId,
    string BuildingName,
    int TotalUnits,
    List<HistoricalMonthData> HistoricalData);

public record ExpensePredictionResponse(
    decimal PredictedNextMonth,
    decimal ConfidenceScore,    // درصد اطمینان (0-100)
    string TrendDirection,      // "increasing", "decreasing", "stable"
    string Analysis,            // تحلیل متنی Gemini
    List<string> Recommendations);

public interface IExpensePredictionAIService
{
    Task<ExpensePredictionResponse> PredictNextMonthAsync(
        ExpensePredictionRequest request,
        CancellationToken cancellationToken = default);
}