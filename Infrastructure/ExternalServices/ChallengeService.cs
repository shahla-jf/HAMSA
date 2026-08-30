using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HAMSA.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HAMSA.Infrastructure.ExternalServices;

public class GroupChallengeAiService : IGroupChallengeAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GroupChallengeAiService> _logger;

    private readonly string _challengeWebhookUrl;
    private readonly string _username;
    private readonly string _password;
    
    // ✅ تایم‌اوت طولانی‌تر برای Gemini (که گاهی کند است)
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(5);

    public GroupChallengeAiService(
        IHttpClientFactory factory,
        IConfiguration configuration,
        ILogger<GroupChallengeAiService> logger)
    {
        _httpClientFactory = factory;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = configuration["N8N:BaseUrl"]
            ?? throw new InvalidOperationException("N8N:BaseUrl is not configured.");
            
        _challengeWebhookUrl = $"{baseUrl.TrimEnd('/')}/webhook/generate-challenge";
        _username = configuration["N8N:Username"]
            ?? throw new InvalidOperationException("N8N:Username is not configured.");
        _password = configuration["N8N:Password"]
            ?? throw new InvalidOperationException("N8N:Password is not configured.");
    }

    public async Task<AiChallengeResponse> GenerateChallengeAsync(
        Guid buildingId,
        List<ParticipantProfile> participants,
        CancellationToken cancellationToken = default)
    {
        // ✅ استفاده از HttpClient با نام مشخص برای پایداری بهتر
        var client = _httpClientFactory.CreateClient("GroupChallengeClient");
        client.Timeout = RequestTimeout;

        // Basic Authentication
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var request = new
        {
            buildingId = buildingId.ToString(),
            participants = participants.Select(p => new
            {
                age = p.Age,
                gender = p.Gender.ToString(),
                sportsBackground = p.SportsBackground
            }).ToList(),
            totalParticipants = participants.Count,
            averageAge = participants.Count > 0 ? (int)participants.Average(p => p.Age) : 0
        };

        try
        {
            _logger.LogInformation(
                "Requesting AI challenge for building {BuildingId} with {Count} participants",
                buildingId, participants.Count);

            var response = await client.PostAsJsonAsync(_challengeWebhookUrl, request, cancellationToken);
            response.EnsureSuccessStatusCode();

            // ✅ استفاده از JsonSerializerOptions برای اطمینان از case-insensitive
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var result = await response.Content.ReadFromJsonAsync<AiChallengeResponse>(options, cancellationToken);

            if (result is null || string.IsNullOrWhiteSpace(result.Title))
            {
                _logger.LogWarning("AI service returned empty response for building {BuildingId}", buildingId);
                return GetFallbackChallenge();
            }

            _logger.LogInformation(
                "AI challenge generated for building {BuildingId}: {Title}",
                buildingId, result.Title);

            return result;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, 
                "AI service timeout for building {BuildingId} (Timeout: {Timeout} minutes)", 
                buildingId, RequestTimeout.TotalMinutes);
            return GetFallbackChallenge();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI service HTTP error for building {BuildingId}", buildingId);
            return GetFallbackChallenge();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "AI service returned invalid JSON for building {BuildingId}", buildingId);
            return GetFallbackChallenge();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling AI service for building {BuildingId}", buildingId);
            return GetFallbackChallenge();
        }
    }

    private static AiChallengeResponse GetFallbackChallenge()
        => new(
            "چالش گروهی این هفته",
            "۳۰ دقیقه فعالیت بدنی سبک تا متوسط انجام دهید. می‌توانید پیاده‌روی سریع، کشش یا حرکات کششی در منزل انجام دهید. پس از اتمام، دکمه «انجام دادم» را در اپلیکیشن بزنید. ایمنی و تناسب با توان شخصی را در اولویت قرار دهید.");
}