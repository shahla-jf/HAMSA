using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.RemoveOneTenant;

// ------- Command -------
public record RemoveOneTenantCommand(
    Guid BuildingId,
    Guid RequestingOwnerId,
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
        // پیدا کردن واحد
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);
        if (unit is null)
            return new RemoveOneTenantResult(false, "واحدی با این مشخصات یافت نشد");

        var unitMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);

        // بررسی مالک بودن درخواست‌دهنده برای همین واحد
        var isOwner = unitMemberships.Any(m =>
            m.UserId == command.RequestingOwnerId &&
            m.Role == UserRole.Owner &&
            m.IsActive);

        if (!isOwner)
            return new RemoveOneTenantResult(false, "شما مالک این واحد نیستید و اجازه حذف مستاجر آن را ندارید");

        // پیدا کردن کاربر مستاجر با شماره موبایل
        var tenant = await _userRepository.GetByPhoneNumberAsync(command.TenantPhoneNumber);
        if (tenant is null)
            return new RemoveOneTenantResult(false, "کاربری با این شماره موبایل یافت نشد");

        // پیدا کردن عضویت فعال همین کاربر به عنوان مستاجر همین واحد
        var targetMembership = unitMemberships.FirstOrDefault(m =>
            m.UserId == tenant.Id &&
            m.Role == UserRole.Tenant &&
            m.IsActive);

        if (targetMembership is null)
            return new RemoveOneTenantResult(false, "این کاربر مستاجر فعال این واحد نیست");

        targetMembership.Deactivate();
        _membershipRepository.Update(targetMembership);
        await _membershipRepository.SaveChangesAsync();

        return new RemoveOneTenantResult(true, "مستاجر با موفقیت از واحد حذف شد");
    }
}