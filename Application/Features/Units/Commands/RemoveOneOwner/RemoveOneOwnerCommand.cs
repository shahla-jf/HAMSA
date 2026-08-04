using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.RemoveOneOwner;

// ------- Command -------
public record RemoveOneOwnerCommand(
    Guid BuildingId,
    Guid RequestingUserId, // مدیر ساختمان یا یکی از مالک‌های فعال همین واحد
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
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);
        if (unit is null)
            return new RemoveOneOwnerResult(false, "واحدی با این مشخصات یافت نشد");

        var unitMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);

        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        var isManager = managerId == command.RequestingUserId;

        // آیا درخواست‌دهنده خودش یک مالک اصلی فعال همین واحد است
        var isOwnerOfUnit = unitMemberships.Any(m =>
            m.UserId == command.RequestingUserId &&
            m.Role == UserRole.Owner &&
            m.IsActive &&
            m.IsPrimary);

        if (!isManager && !isOwnerOfUnit)
            return new RemoveOneOwnerResult(false, "شما اجازه حذف مالک این واحد را ندارید");

        var owner = await _userRepository.GetByPhoneNumberAsync(command.OwnerPhoneNumber);
        if (owner is null)
            return new RemoveOneOwnerResult(false, "کاربری با این شماره موبایل یافت نشد");

        var targetMembership = unitMemberships.FirstOrDefault(m =>
            m.UserId == owner.Id &&
            m.Role == UserRole.Owner &&
            m.IsActive);

        if (targetMembership is null)
            return new RemoveOneOwnerResult(false, "این کاربر مالک فعال این واحد نیست");

        // اگه درخواست‌دهنده مدیر نیست (یعنی خودش یکی از مالک‌هاست)،
        // فقط اجازه داره مالک فرعی رو حذف کنه، نه مالک اصلی
        if (!isManager && targetMembership.IsPrimary)
            return new RemoveOneOwnerResult(false, "مالک اصلی فقط توسط مدیر ساختمان قابل حذف است");

        targetMembership.Deactivate();
        _membershipRepository.Update(targetMembership);
        await _membershipRepository.SaveChangesAsync();

        return new RemoveOneOwnerResult(true, "مالک با موفقیت از واحد حذف شد");
    }
}