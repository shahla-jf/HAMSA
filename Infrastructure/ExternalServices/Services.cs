using HAMSA.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace HAMSA.Infrastructure.ExternalServices;

public class SmsService : ISmsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SmsService> _logger;
    private readonly string _baseUrl;
    private readonly string _webhookUrl;
    private readonly string _username;
    private readonly string _password;

    public SmsService(IHttpClientFactory httpClientFactory, ILogger<SmsService> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        _baseUrl = configuration["N8N:BaseUrl"]!;
        _webhookUrl = $"{_baseUrl}/webhook/send-otp";
        _username = configuration["N8N:Username"]!;
        _password = configuration["N8N:Password"]!;
    }

    public async Task SendOtpAsync(string phoneNumber, string otpcode)
    {
        try
        {
            _logger.LogInformation(">>> Starting SendOtpAsync for {Phone}", phoneNumber);
            _logger.LogInformation("Webhook URL: {Url}", _webhookUrl);

            var client = _httpClientFactory.CreateClient();

            var request = new { phoneNumber, code = otpcode };

            var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            _logger.LogInformation("Sending request to n8n...");

            var response = await client.PostAsJsonAsync(_webhookUrl, request);

            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "n8n response: Status={StatusCode}, Body={Body}",
                response.StatusCode,
                responseBody);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"n8n returned {(int)response.StatusCode}: {responseBody}");
            }

            _logger.LogInformation("OTP sent successfully to {Phone}", phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP via n8n to {Phone}", phoneNumber);
            throw;
        }
    }
}

// --- JWT Service ---
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string phoneNumber)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.MobilePhone, phoneNumber),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true
            }, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim is null ? null : Guid.Parse(userIdClaim);
        }
        catch
        {
            return null;
        }
    }
}
