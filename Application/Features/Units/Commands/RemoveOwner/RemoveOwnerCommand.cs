using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.RemoveOwner;

// ------- Command -------
public record RemoveOwnerCommand(
    Guid BuildingId,
    Guid RequestingManagerId,
    int Block,
    int Floor,
    int UnitNumber
);

// ------- Result -------
public record RemoveOwnerResult(bool Success, string Message, int RemovedCount = 0);

// ------- Handler -------
public class RemoveOwnerHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;

    public RemoveOwnerHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
    }

    public async Task<RemoveOwnerResult> HandleAsync(RemoveOwnerCommand command)
    {
        // بررسی مدیر بودن
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.RequestingManagerId)
            return new RemoveOwnerResult(false, "فقط مدیر ساختمان می‌تواند مالک را حذف کند");

        // پیدا کردن واحد
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);

        if (unit is null)
            return new RemoveOwnerResult(false, "واحدی با این مشخصات یافت نشد");

        // گرفتن همه‌ی افراد این واحد (مالک + مستاجرین) که در این ساختمان عضو هستن
        var memberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);
        var activeMemberships = memberships.Where(m => m.IsActive).ToList();

        if (!activeMemberships.Any())
            return new RemoveOwnerResult(false, "این واحد در حال حاضر عضو فعالی ندارد");

        // حذف (غیرفعال کردن) همه‌ی افراد این واحد از ساختمان
        foreach (var m in activeMemberships)
        {
            m.Deactivate();
            _membershipRepository.Update(m);
        }
        await _membershipRepository.SaveChangesAsync();

        return new RemoveOwnerResult(true, "مالک و مستاجرین واحد با موفقیت از ساختمان حذف شدند", activeMemberships.Count);
    }
}