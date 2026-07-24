using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.User.Commands.UpdateProfile;

// ------- Command -------
public record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName
);

// ------- Result -------
public record UpdateProfileResult(bool Success, string Message);

// ------- Handler -------
public class UpdateProfileHandler
{
    private readonly IUserRepository _userRepository;

    public UpdateProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UpdateProfileResult> HandleAsync(UpdateProfileCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            return new UpdateProfileResult(false, "کاربری با این مشخصات یافت نشد");

        if (string.IsNullOrWhiteSpace(command.FirstName))
            return new UpdateProfileResult(false, "نام نمی‌تواند خالی باشد");

        if (string.IsNullOrWhiteSpace(command.LastName))
            return new UpdateProfileResult(false, "نام خانوادگی نمی‌تواند خالی باشد");

        // شماره موبایل و عکس پروفایل رو دست‌نخورده نگه می‌داریم، فقط اسم/فامیل آپدیت می‌شه
        user.UpdateProfile(
            command.FirstName.Trim(),
            command.LastName.Trim(),
            user.PhoneNumber,
            user.ProfileImageUrl);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return new UpdateProfileResult(true, "پروفایل با موفقیت به‌روزرسانی شد");
    }
}