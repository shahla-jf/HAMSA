using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.UpdateOwner;

// ------- Command -------
public record EditOwnerCommand(
    Guid BuildingId,
    Guid RequestingUserId, 
    int Block,
    int Floor,
    int UnitNumber,
    DateTime? NewStartDate,
    DateTime? NewEndDate,
    int? NewBlock = null,
    int? NewFloor = null,
    int? NewUnitNumber = null,
    Guid? TargetUserId = null 
);

// ------- Result -------
public record EditOwnerResult(bool Success, string Message, int UpdatedCount = 0);

// ------- Handler -------
public class EditOwnerHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;

    public EditOwnerHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
    }

    public async Task<EditOwnerResult> HandleAsync(EditOwnerCommand command)
    { 
        Console.WriteLine($"[DEBUG] Command Received -> BuildingId: {command.BuildingId}, Block: {command.Block}, Floor: {command.Floor}, UnitNumber: {command.UnitNumber}");
    
        var sourceUnit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);

        if (sourceUnit is null)
            return new EditOwnerResult(false, "واحد مبدأ با این مشخصات یافت نشد");

        var sourceMemberships = (await _membershipRepository.GetByUnitIdAsync(sourceUnit.Id)).ToList();

        // ۱. بررسی دسترسی درخواست‌دهنده در واحد مبدأ
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        var isManager = managerId == command.RequestingUserId;

        var isPrimaryOwnerOfSource = sourceMemberships.Any(m =>
            m.UserId == command.RequestingUserId &&
            m.Role == UserRole.Owner &&
            m.IsActive &&
            m.IsPrimary);

        if (!isManager && !isPrimaryOwnerOfSource)
            return new EditOwnerResult(false, "فقط مدیر ساختمان یا مالک اصلی این واحد می‌تواند مشخصات مالکان را ویرایش کند");

        // ۲. بررسی تغییر واحد (آیا پارامترهای واحد جدید ارسال شده و متفاوت هستند؟)
        bool isChangingUnit = command.NewBlock.HasValue && command.NewFloor.HasValue && command.NewUnitNumber.HasValue &&
                               (command.NewBlock != command.Block || command.NewFloor != command.Floor || command.NewUnitNumber != command.UnitNumber);

        Unit? targetUnit = null;
        bool isNewUnitCreated = false;

        if (isChangingUnit)
        {
            targetUnit = await _unitRepository.GetByBlockFloorUnitAsync(
                command.BuildingId, command.NewBlock.Value, command.NewFloor.Value, command.NewUnitNumber.Value);

            if (targetUnit is null && isManager)
            {
                targetUnit = Unit.Create(command.BuildingId, command.NewBlock.Value, command.NewFloor.Value, command.NewUnitNumber.Value);
                await _unitRepository.AddAsync(targetUnit);
                isNewUnitCreated = true;
            }

            // اگر مدیر نیست، باید مالک واحد مقصد هم باشد
            if (!isManager)
            {
                if (isNewUnitCreated)
                {
                    return new EditOwnerResult(false, "واحد مقصد وجود ندارد و شما به عنوان مالک عادی اجازه ساخت واحد جدید را ندارید.");
                }

                var targetMemberships = await _membershipRepository.GetByUnitIdAsync(targetUnit.Id);
                var isOwnerOfTarget = targetMemberships.Any(m =>
                    m.UserId == command.RequestingUserId &&
                    m.Role == UserRole.Owner &&
                    m.IsActive);

                if (!isOwnerOfTarget)
                    return new EditOwnerResult(false, "مالک اصلی برای انتقال مالک دیگر، باید خودش نیز مالک واحد مقصد باشد");
            }
        }

        // ۳. فیلتر کردن مالکان برای ویرایش
        var membersToUpdate = sourceMemberships
            .Where(m => m.Role == UserRole.Owner && m.IsActive)
            .ToList();

        if (command.TargetUserId.HasValue)
            membersToUpdate = membersToUpdate.Where(m => m.UserId == command.TargetUserId.Value).ToList();

        if (!membersToUpdate.Any())
            return new EditOwnerResult(false, "مالک فعالی برای ویرایش در این واحد یافت نشد");

        // ۴. اعمال تغییرات
        foreach (var owner in membersToUpdate)
        {
            if (command.NewStartDate.HasValue)
                owner.UpdateStartDate(command.NewStartDate.Value);
            if (command.NewEndDate.HasValue)
                owner.SetEndDate(command.NewEndDate.Value);
            if (isChangingUnit)
                owner.SetUnitId(targetUnit!.Id);

            _membershipRepository.Update(owner);
        }

        await _membershipRepository.SaveChangesAsync();
        return new EditOwnerResult(true, "مشخصات مالکان با موفقیت ویرایش شد", membersToUpdate.Count);
    }
}