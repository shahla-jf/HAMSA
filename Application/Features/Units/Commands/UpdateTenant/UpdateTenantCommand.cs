using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.UpdateTenant;

// ------- Command -------
public record EditTenantCommand(
    Guid BuildingId,
    Guid RequestingUserId, // تغییر نام برای پوشش مالک و مستاجر اصلی
    int Block,
    int Floor,
    int UnitNumber,
    DateTime? NewStartDate,
    DateTime? NewEndDate,
    int? NewBlock = null,
    int? NewFloor = null,
    int? NewUnitNumber = null,
    Guid? TargetUserId = null // اختیاری: اگر نال باشد، روی همه مستاجران فعال واحد اعمال می‌شود
);

// ------- Result -------
public record EditTenantResult(bool Success, string Message, int UpdatedCount = 0);

// ------- Handler -------
public class EditTenantHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;

    public EditTenantHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
    }

    public async Task<EditTenantResult> HandleAsync(EditTenantCommand command)
    {
        var sourceUnit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);

        if (sourceUnit is null)
            return new EditTenantResult(false, "واحد مبدأ با این مشخصات یافت نشد");

        var sourceMemberships = (await _membershipRepository.GetByUnitIdAsync(sourceUnit.Id)).ToList();

        // ۱. بررسی دسترسی درخواست‌دهنده در واحد مبدأ
        // (نکته: اگر می‌خواهید هر مالکی اجازه این کار را داشته باشد IsPrimary را بردارید، اما من برای حفظ امنیت کد قبلی‌تان آن را نگه داشتم)
        var isOwnerOfSource = sourceMemberships.Any(m =>
            m.UserId == command.RequestingUserId &&
            m.Role == UserRole.Owner &&
            m.IsActive && 
            m.IsPrimary); 

        var isPrimaryTenantOfSource = sourceMemberships.Any(m =>
            m.UserId == command.RequestingUserId &&
            m.Role == UserRole.Tenant &&
            m.IsActive &&
            m.IsPrimary);

        if (!isOwnerOfSource && !isPrimaryTenantOfSource)
            return new EditTenantResult(false, "فقط مالک اصلی یا مستاجر اصلی این واحد می‌تواند مشخصات مستاجران را ویرایش کند");

        // ۲. بررسی تغییر واحد
        bool isChangingUnit = command.NewBlock.HasValue && command.NewFloor.HasValue && command.NewUnitNumber.HasValue &&
                               (command.NewBlock != command.Block || command.NewFloor != command.Floor || command.NewUnitNumber != command.UnitNumber);

        Unit? targetUnit = null;
        if (isChangingUnit)
        {
            targetUnit = await _unitRepository.GetByBlockFloorUnitAsync(
                command.BuildingId, command.NewBlock.Value, command.NewFloor.Value, command.NewUnitNumber.Value);

            if (targetUnit is null)
                return new EditTenantResult(false, "واحد مقصد با این مشخصات یافت نشد");

            var targetMemberships = await _membershipRepository.GetByUnitIdAsync(targetUnit.Id);

            if (isOwnerOfSource)
            {
                // شرط شما: مالک باید مالک واحد مقصد هم باشد
                var isOwnerOfTarget = targetMemberships.Any(m =>
                    m.UserId == command.RequestingUserId &&
                    m.Role == UserRole.Owner &&
                    m.IsActive);

                if (!isOwnerOfTarget)
                    return new EditTenantResult(false, "مالک برای انتقال مستاجر، باید خودش نیز مالک واحد مقصد باشد");
            }
            else if (isPrimaryTenantOfSource)
            {
                // شرط شما: مستاجر اصلی باید مستاجر اصلی واحد مقصد هم باشد
                var isPrimaryTenantOfTarget = targetMemberships.Any(m =>
                    m.UserId == command.RequestingUserId &&
                    m.Role == UserRole.Tenant &&
                    m.IsActive &&
                    m.IsPrimary);

                if (!isPrimaryTenantOfTarget)
                    return new EditTenantResult(false, "مستاجر اصلی برای انتقال مستاجر دیگر، باید خودش نیز مستاجر اصلی واحد مقصد باشد");
            }
        }

        // ۳. فیلتر کردن مستاجران برای ویرایش
        var membersToUpdate = sourceMemberships
            .Where(m => m.Role == UserRole.Tenant && m.IsActive && m.IsResident)
            .ToList();

        if (command.TargetUserId.HasValue)
            membersToUpdate = membersToUpdate.Where(m => m.UserId == command.TargetUserId.Value).ToList();

        if (!membersToUpdate.Any())
            return new EditTenantResult(false, "مستاجر ساکن و فعالی برای ویرایش در این واحد یافت نشد");

        // ۴. اعمال تغییرات
        foreach (var tenant in membersToUpdate)
        {
            if (command.NewStartDate.HasValue)
                tenant.UpdateStartDate(command.NewStartDate.Value);
            if (command.NewEndDate.HasValue)
                tenant.SetEndDate(command.NewEndDate.Value);
            if (isChangingUnit)
                tenant.SetUnitId(targetUnit!.Id);

            _membershipRepository.Update(tenant);
        }

        await _membershipRepository.SaveChangesAsync();
        return new EditTenantResult(true, "مشخصات مستاجران با موفقیت ویرایش شد", membersToUpdate.Count);
    }
}