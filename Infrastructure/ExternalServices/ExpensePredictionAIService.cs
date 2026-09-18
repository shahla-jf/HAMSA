using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HAMSA.Domain.Interfaces.Services;

namespace HAMSA.Infrastructure.ExternalServices;

public class ExpensePredictionAIService : IExpensePredictionAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpensePredictionAIService> _logger;

    private readonly string _predictionWebhookUrl;
    private readonly string _username;
    private readonly string _password;

    private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(5);

    public ExpensePredictionAIService(
        IHttpClientFactory factory,
        IConfiguration configuration,
        ILogger<ExpensePredictionAIService> logger)
    {
        _httpClientFactory = factory;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = configuration["N8N:BaseUrl"]
            ?? throw new InvalidOperationException("N8N:BaseUrl is not configured.");

        _predictionWebhookUrl = $"{baseUrl.TrimEnd('/')}/webhook/predict-expenses";
        _username = configuration["N8N:Username"]
            ?? throw new InvalidOperationException("N8N:Username is not configured.");
        _password = configuration["N8N:Password"]
            ?? throw new InvalidOperationException("N8N:Password is not configured.");
    }

    public async Task<ExpensePredictionResponse> PredictNextMonthAsync(
        ExpensePredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ExpensePredictionClient");
        client.Timeout = RequestTimeout;

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        try
        {
            _logger.LogInformation(
                "Requesting expense prediction for building {BuildingId} with {Months} months of data",
                request.BuildingId, request.HistoricalData.Count);

            var response = await client.PostAsJsonAsync(_predictionWebhookUrl, request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await response.Content.ReadFromJsonAsync<ExpensePredictionResponse>(options, cancellationToken);

            if (result is null || string.IsNullOrWhiteSpace(result.Analysis))
            {
                _logger.LogWarning("Prediction service returned empty response");
                return GetFallbackPrediction(request);
            }

            return result;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Prediction service timeout");
            return GetFallbackPrediction(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Prediction service HTTP error");
            return GetFallbackPrediction(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in prediction service");
            return GetFallbackPrediction(request);
        }
    }

    private static ExpensePredictionResponse GetFallbackPrediction(ExpensePredictionRequest request)
    {
        // پیش‌بینی ساده: میانگین ۳ ماه اخیر
        var lastThree = request.HistoricalData.TakeLast(3).ToList();
        var avg = lastThree.Count > 0 ? lastThree.Average(h => h.TotalCosts) : 0;

        return new ExpensePredictionResponse(
            avg,
            50,
            "stable",
            "این پیش‌بینی بر اساس میانگین ساده ۳ ماه اخیر است.",
            new List<string> { "ثبت منظم هزینه‌ها برای پیش‌بینی دقیق‌تر ضروری است." });
    }
}