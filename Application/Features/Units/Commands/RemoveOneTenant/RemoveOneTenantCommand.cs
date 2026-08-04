using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.RemoveOneTenant;

// ------- Command -------
public record RemoveOneTenantCommand(
    Guid BuildingId,
    Guid RequestingUserId, // مالک واحد یا مستاجر اصلی همین واحد
    string TenantPhoneNumber,
    int Block,
    int Floor,
    int UnitNumber
);

// ------- Result -------
public record RemoveOneTenantResult(bool Success, string Message);

// ------- Handler -------
public class RemoveOneTenantHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;

    public RemoveOneTenantHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository,
        IUserRepository userRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
        _userRepository = userRepository;
    }

    public async Task<RemoveOneTenantResult> HandleAsync(RemoveOneTenantCommand command)
    {
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);
        if (unit is null)
            return new RemoveOneTenantResult(false, "واحدی با این مشخصات یافت نشد");

        var unitMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);

        var isOwnerRequester = unitMemberships.Any(m =>
            m.UserId == command.RequestingUserId &&
            m.Role == UserRole.Owner &&
            m.IsActive);

        var requestingTenantMembership = unitMemberships.FirstOrDefault(m =>
            m.UserId == command.RequestingUserId &&
            m.Role == UserRole.Tenant &&
            m.IsActive);

        var isPrimaryTenantRequester = requestingTenantMembership is { IsPrimary: true };

        if (!isOwnerRequester && !isPrimaryTenantRequester)
            return new RemoveOneTenantResult(false, "شما اجازه حذف مستاجر این واحد را ندارید");

        var tenant = await _userRepository.GetByPhoneNumberAsync(command.TenantPhoneNumber);
        if (tenant is null)
            return new RemoveOneTenantResult(false, "کاربری با این شماره موبایل یافت نشد");

        var targetMembership = unitMemberships.FirstOrDefault(m =>
            m.UserId == tenant.Id &&
            m.Role == UserRole.Tenant &&
            m.IsActive);

        if (targetMembership is null)
            return new RemoveOneTenantResult(false, "این کاربر مستاجر فعال این واحد نیست");

        // اگه درخواست‌دهنده مالک نیست (یعنی خودش مستاجر اصلی است)،
        // فقط اجازه داره مستاجر فرعی رو حذف کنه، نه مستاجر اصلی
        if (!isOwnerRequester && targetMembership.IsPrimary)
            return new RemoveOneTenantResult(false, "مستاجر اصلی فقط توسط مالک واحد قابل حذف است");

        targetMembership.Deactivate();
        _membershipRepository.Update(targetMembership);
        await _membershipRepository.SaveChangesAsync();

        return new RemoveOneTenantResult(true, "مستاجر با موفقیت از واحد حذف شد");
    }
}