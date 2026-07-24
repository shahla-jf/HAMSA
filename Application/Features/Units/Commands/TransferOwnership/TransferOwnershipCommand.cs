using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.TransferOwnership;

// ------- Command -------
public record TransferOwnershipCommand(
    Guid BuildingId,
    Guid RequestingManagerId,
    string NewOwnerPhoneNumber,
    int Block,
    int Floor,
    int UnitNumber,
    bool IsResident
);

// ------- Result -------
public record TransferOwnershipResult(bool Success, string Message, Guid? UnitId = null, int RemovedOwnersCount = 0);

// ------- Handler -------
public class TransferOwnershipHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;

    public TransferOwnershipHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository,
        IUserRepository userRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
        _userRepository = userRepository;
    }

    public async Task<TransferOwnershipResult> HandleAsync(TransferOwnershipCommand command)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.RequestingManagerId)
            return new TransferOwnershipResult(false, "فقط مدیر ساختمان می‌تواند مالکیت را منتقل کند");

        var newOwner = await _userRepository.GetByPhoneNumberAsync(command.NewOwnerPhoneNumber);
        if (newOwner is null)
            return new TransferOwnershipResult(false, "کاربری با این شماره موبایل یافت نشد. لطفاً ابتدا در سیستم ثبت‌نام کند");

        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);

        var removedCount = 0;

        if (unit is null)
        {
            unit = Unit.Create(command.BuildingId, command.Block, command.Floor, command.UnitNumber);
            await _unitRepository.AddAsync(unit);
            await _unitRepository.SaveChangesAsync();
        }
        else
        {
            // اینجا واقعاً می‌خوایم مالکیت عوض بشه: همه‌ی مالک‌های فعلی (همه اعضای خانواده) رو deactivate کن
            var existingMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);
            var currentOwners = existingMemberships.Where(m => m.Role == UserRole.Owner && m.IsActive).ToList();

            foreach (var m in currentOwners)
            {
                m.Deactivate();
                _membershipRepository.Update(m);
            }
            removedCount = currentOwners.Count;
            await _membershipRepository.SaveChangesAsync();

            // نکته: مستاجرین این واحد دست‌نخورده می‌مونن مگه اینکه بخوای اونا رو هم پاک کنی
        }

        var membership = BuildingMembership.Create(
            newOwner.Id, command.BuildingId, unit.Id,
            UserRole.Owner, command.IsResident, DateTime.UtcNow);

        await _membershipRepository.AddAsync(membership);
        await _membershipRepository.SaveChangesAsync();

        return new TransferOwnershipResult(true, "مالکیت واحد با موفقیت منتقل شد", unit.Id, removedCount);
    }
}