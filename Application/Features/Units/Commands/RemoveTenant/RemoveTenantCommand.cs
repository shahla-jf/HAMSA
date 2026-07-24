using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.RemoveTenant;

// ------- Command -------
public record RemoveTenantCommand(
    Guid BuildingId,
    Guid RequestingOwnerId,
    int Block,
    int Floor,
    int UnitNumber
);

// ------- Result -------
public record RemoveTenantResult(bool Success, string Message, int RemovedCount = 0);

// ------- Handler -------
public class RemoveTenantHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;

    public RemoveTenantHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
    }

    public async Task<RemoveTenantResult> HandleAsync(RemoveTenantCommand command)
    {
        // پیدا کردن واحد
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);

        if (unit is null)
            return new RemoveTenantResult(false, "واحدی با این مشخصات یافت نشد");

        // همه‌ی عضویت‌های فعال این واحد
        var memberships = (await _membershipRepository.GetByUnitIdAsync(unit.Id)).ToList();

        // بررسی اینکه درخواست‌دهنده، مالک فعال همین واحد هست
        var isOwnerOfUnit = memberships.Any(m =>
            m.UserId == command.RequestingOwnerId &&
            m.Role == UserRole.Owner &&
            m.IsActive);

        if (!isOwnerOfUnit)
            return new RemoveTenantResult(false, "شما مالک این واحد نیستید و اجازه حذف مستاجر آن را ندارید");

        // فقط مستاجرین همین واحد
        var activeTenants = memberships
            .Where(m => m.Role == UserRole.Tenant && m.IsActive)
            .ToList();

        if (!activeTenants.Any())
            return new RemoveTenantResult(false, "این واحد در حال حاضر مستاجر فعالی ندارد");

        foreach (var m in activeTenants)
        {
            m.Deactivate();
            _membershipRepository.Update(m);
        }
        await _membershipRepository.SaveChangesAsync();

        return new RemoveTenantResult(true, "مستاجر(ها) با موفقیت از واحد حذف شدند", activeTenants.Count);
    }
}