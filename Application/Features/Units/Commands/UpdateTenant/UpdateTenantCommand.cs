using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.UpdateTenant;

// ------- Command -------
public record EditTenantDatesCommand(
    Guid BuildingId,
    Guid RequestingOwnerId, // باید یکی از مالک‌های فعال همین واحد باشه
    int Block,
    int Floor,
    int UnitNumber,
    DateTime? NewStartDate,
    DateTime? NewEndDate
);

// ------- Result -------
public record EditTenantDatesResult(bool Success, string Message, int UpdatedCount = 0);

// ------- Handler -------
public class EditTenantDatesHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;

    public EditTenantDatesHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
    }

    public async Task<EditTenantDatesResult> HandleAsync(EditTenantDatesCommand command)
    {
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);

        if (unit is null)
            return new EditTenantDatesResult(false, "واحدی با این مشخصات یافت نشد");

        var unitMemberships = (await _membershipRepository.GetByUnitIdAsync(unit.Id)).ToList();

        // بررسی اینکه درخواست‌دهنده مالک فعال همین واحد است
        var isOwnerOfUnit = unitMemberships.Any(m =>
            m.UserId == command.RequestingOwnerId &&
            m.Role == UserRole.Owner &&
            m.IsActive && m.IsPrimary);

        if (!isOwnerOfUnit)
            return new EditTenantDatesResult(false, "فقط مالک اصلی این واحد می‌تواند تاریخ مستاجران را ویرایش کند");

        // فقط مستاجرهای ساکن و فعال همین واحد
        var activeResidentTenants = unitMemberships
            .Where(m => m.Role == UserRole.Tenant && m.IsActive && m.IsResident)
            .ToList();

        if (!activeResidentTenants.Any())
            return new EditTenantDatesResult(false, "این واحد در حال حاضر مستاجر ساکن فعالی ندارد");

        foreach (var tenant in activeResidentTenants)
        {
            if(command.NewStartDate != null)
                tenant.UpdateStartDate(command.NewStartDate.Value);
            if (command.NewEndDate != null)
                tenant.SetEndDate(command.NewEndDate.Value);
            _membershipRepository.Update(tenant);
        }

        await _membershipRepository.SaveChangesAsync();

        return new EditTenantDatesResult(true, "تاریخ مستاجران با موفقیت ویرایش شد", activeResidentTenants.Count);
    }
}