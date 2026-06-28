using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Domain.Interfaces.Services;

namespace HAMSA.Application.Features.Auth.Commands.VerifyOtp;

// ------- Command -------
public record VerifyOtpCommand(string PhoneNumber, string Code);

// ------- Result -------
public record VerifyOtpResult(bool Success, string Message, string? Token = null, bool IsNewUser = false);

// ------- Handler -------
public class VerifyOtpHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IJwtService _jwtService;

    public VerifyOtpHandler(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _jwtService = jwtService;
    }

    public async Task<VerifyOtpResult> HandleAsync(VerifyOtpCommand command)
    {
        var otp = await _otpRepository.GetValidOtpAsync(command.PhoneNumber, command.Code);
        if (otp is null)
            return new VerifyOtpResult(false, "کد تایید نامعتبر یا منقضی شده است");

        otp.MarkAsUsed();
        _otpRepository.Update(otp);
        await _otpRepository.SaveChangesAsync();

        var user = await _userRepository.GetByPhoneNumberAsync(command.PhoneNumber);
        if (user is null)
            return new VerifyOtpResult(false, "کاربر یافت نشد");

        var isNewUser = string.IsNullOrEmpty(user.FirstName);

        var token = _jwtService.GenerateToken(user.Id, user.PhoneNumber);

        return new VerifyOtpResult(true, "ورود موفق", token, isNewUser);
    }
}
