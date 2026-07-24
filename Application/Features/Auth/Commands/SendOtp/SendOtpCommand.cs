using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Domain.Interfaces.Services;

namespace HAMSA.Application.Features.Auth.Commands.SendOtp;

// ------- Command -------
public record SendOtpCommand(string PhoneNumber);

// ------- Result -------
public record SendOtpResult(bool Success, string Message);

// ------- Handler -------
public class SendOtpHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly ISmsService _smsService;

    public SendOtpHandler(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        ISmsService smsService)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _smsService = smsService;
    }

    public async Task<SendOtpResult> HandleAsync(SendOtpCommand command)
    {
        var userExists = await _userRepository.ExistsAsync(command.PhoneNumber);
        if (!userExists)
        {
            var newUser = HAMSA.Domain.Entities.User.Create(command.PhoneNumber);
            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();
        }

        await _otpRepository.InvalidatePreviousAsync(command.PhoneNumber);

        var otp = OtpCode.Create(command.PhoneNumber);
        await _otpRepository.AddAsync(otp);
        await _otpRepository.SaveChangesAsync();
        
        try
        {
            await _smsService.SendOtpAsync(command.PhoneNumber, otp.Code);
            return new SendOtpResult(true, "کد تایید ارسال شد");
        }
        catch (Exception)
        {
            return new SendOtpResult(false, "ارسال با مشکل مواجه شد");
        }
    }
}
