using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.UpdateOwner;

// ------- Command -------
public record EditOwnerDatesCommand(
    Guid BuildingId,
    Guid RequestingManagerId,
    int Block,
    int Floor,
    int UnitNumber,
    DateTime? NewStartDate,
    DateTime? NewEndDate
);

// ------- Result -------
public record EditOwnerDatesResult(bool Success, string Message, int UpdatedCount = 0);

// ------- Handler -------
public class EditOwnerDatesHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;

    public EditOwnerDatesHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
    }

    public async Task<EditOwnerDatesResult> HandleAsync(EditOwnerDatesCommand command)
    {
        // فقط مدیر ساختمان اجازه داره
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.RequestingManagerId)
            return new EditOwnerDatesResult(false, "فقط مدیر ساختمان می‌تواند تاریخ مالکان را ویرایش کند");

        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);

        if (unit is null)
            return new EditOwnerDatesResult(false, "واحدی با این مشخصات یافت نشد");

        var unitMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);

        var activeOwners = unitMemberships
            .Where(m => m.Role == UserRole.Owner && m.IsActive)
            .ToList();

        if (!activeOwners.Any())
            return new EditOwnerDatesResult(false, "این واحد در حال حاضر مالک فعالی ندارد");

        foreach (var owner in activeOwners)
        {
            if(command.NewStartDate != null)
                owner.UpdateStartDate(command.NewStartDate.Value);
            if (command.NewEndDate != null)
                owner.SetEndDate(command.NewEndDate.Value);
            _membershipRepository.Update(owner);
        }

        await _membershipRepository.SaveChangesAsync();

        return new EditOwnerDatesResult(true, "تاریخ مالکان با موفقیت ویرایش شد", activeOwners.Count);
    }
}