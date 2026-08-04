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

        // اگه فرستاده نشده یا خالیه، مقدار فعلی دیتابیس رو نگه می‌داریم
        var newFirstName = string.IsNullOrWhiteSpace(command.FirstName)
            ? user.FirstName
            : command.FirstName.Trim();

        var newLastName = string.IsNullOrWhiteSpace(command.LastName)
            ? user.LastName
            : command.LastName.Trim();

        if (string.IsNullOrWhiteSpace(newFirstName))
            return new UpdateProfileResult(false, "نام نمی‌تواند خالی باشد");

        if (string.IsNullOrWhiteSpace(newLastName))
            return new UpdateProfileResult(false, "نام خانوادگی نمی‌تواند خالی باشد");
        
        user.UpdateProfile(
            newFirstName,
            newLastName,
            user.PhoneNumber,
            user.ProfileImageUrl);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return new UpdateProfileResult(true, "پروفایل با موفقیت به‌روزرسانی شد");
    }
}