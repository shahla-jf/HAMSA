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

    public AuthController(SendOtpHandler sendOtpHandler, VerifyOtpHandler verifyOtpHandler)
    {
        _sendOtpHandler = sendOtpHandler;
        _verifyOtpHandler = verifyOtpHandler;
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
}

// --- Request Models ---
public record SendOtpRequest(string PhoneNumber);
public record VerifyOtpRequest(string PhoneNumber, string Code);
