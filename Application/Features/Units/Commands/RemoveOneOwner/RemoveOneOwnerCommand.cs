using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.RemoveOneOwner;

// ------- Command -------
public record RemoveOneOwnerCommand(
    Guid BuildingId,
    Guid RequestingManagerId,
    string OwnerPhoneNumber,
    int Block,
    int Floor,
    int UnitNumber
);

// ------- Result -------
public record RemoveOneOwnerResult(bool Success, string Message);

// ------- Handler -------
public class RemoveOneOwnerHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;

    public RemoveOneOwnerHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository,
        IUserRepository userRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
        _userRepository = userRepository;
    }

    public async Task<RemoveOneOwnerResult> HandleAsync(RemoveOneOwnerCommand command)
    {
        // بررسی مدیر بودن
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.RequestingManagerId)
            return new RemoveOneOwnerResult(false, "فقط مدیر ساختمان می‌تواند مالک را حذف کند");

        // پیدا کردن واحد
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);
        if (unit is null)
            return new RemoveOneOwnerResult(false, "واحدی با این مشخصات یافت نشد");

        // پیدا کردن کاربر مالک با شماره موبایل
        var owner = await _userRepository.GetByPhoneNumberAsync(command.OwnerPhoneNumber);
        if (owner is null)
            return new RemoveOneOwnerResult(false, "کاربری با این شماره موبایل یافت نشد");

        // پیدا کردن عضویت فعال همین کاربر به عنوان مالک همین واحد
        var unitMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);
        var targetMembership = unitMemberships.FirstOrDefault(m =>
            m.UserId == owner.Id &&
            m.Role == UserRole.Owner &&
            m.IsActive);

        if (targetMembership is null)
            return new RemoveOneOwnerResult(false, "این کاربر مالک فعال این واحد نیست");

        targetMembership.Deactivate();
        _membershipRepository.Update(targetMembership);
        await _membershipRepository.SaveChangesAsync();

        return new RemoveOneOwnerResult(true, "مالک با موفقیت از واحد حذف شد");
    }
}