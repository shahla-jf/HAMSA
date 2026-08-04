using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HAMSA.Application.Features.Auth.Commands.Logout;
using HAMSA.Application.Features.Auth.Commands.SendOtp;
using HAMSA.Application.Features.Auth.Commands.VerifyOtp;
using Microsoft.AspNetCore.Mvc;

namespace HAMSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SendOtpHandler _sendOtpHandler;
    private readonly VerifyOtpHandler _verifyOtpHandler;
    private readonly LogoutHandler _logoutHandler;

    public AuthController(SendOtpHandler sendOtpHandler, VerifyOtpHandler verifyOtpHandler, LogoutHandler logoutHandler)
    {
        _sendOtpHandler = sendOtpHandler;
        _verifyOtpHandler = verifyOtpHandler;
        _logoutHandler = logoutHandler;
    }

    /// <summary>
    /// ارسال کد OTP به شماره موبایل
    /// </summary>
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var result = await _sendOtpHandler.HandleAsync(new SendOtpCommand(request.PhoneNumber));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// تایید کد OTP و دریافت JWT
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _verifyOtpHandler.HandleAsync(
            new VerifyOtpCommand(request.PhoneNumber, request.Code));

        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        if (jti is null || userIdClaim is null || expClaim is null)
            return Unauthorized(new { message = "توکن نامعتبر است" });

        var expiresAt = DateTimeOffset
            .FromUnixTimeSeconds(long.Parse(expClaim, System.Globalization.CultureInfo.InvariantCulture))
            .UtcDateTime;

        var result = await _logoutHandler.HandleAsync(
            new LogoutCommand(jti, Guid.Parse(userIdClaim), expiresAt));

        return Ok(result);
    }
    
}

// --- Request Models ---
public record SendOtpRequest(string PhoneNumber);
public record VerifyOtpRequest(string PhoneNumber, string Code);
