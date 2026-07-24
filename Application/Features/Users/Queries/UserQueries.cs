using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.User.Queries;

// ------- Query -------
public record GetProfileQuery(Guid UserId);

// ------- Result -------
public record GetProfileResult(
    bool Success,
    string Message,
    string FirstName,
    string LastName
);

// ------- Handler -------
public class GetProfileHandler
{
    private readonly IUserRepository _userRepository;

    public GetProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetProfileResult> HandleAsync(GetProfileQuery query)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId);

        if (user is null)
            return new GetProfileResult(
                false,
                "کاربری با این مشخصات یافت نشد",
                string.Empty,
                string.Empty);

        return new GetProfileResult(
            true,
            "اطلاعات پروفایل دریافت شد",
            user.FirstName,
            user.LastName);
    }
}