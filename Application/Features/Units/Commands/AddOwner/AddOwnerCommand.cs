using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.AddOwner;

// ------- Command -------
public record AddOwnerCommand(
    Guid BuildingId,
    Guid RequestingManagerId,
    string OwnerPhoneNumber,
    int Block,
    int Floor,
    int UnitNumber,
    bool IsResident   // ساکنه یا موجر
);

// ------- Result -------
public record AddOwnerResult(bool Success, string Message, Guid? UnitId = null);

// ------- Handler -------
public class AddOwnerHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;

    public AddOwnerHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository,
        IUserRepository userRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
        _userRepository = userRepository;
    }

    public async Task<AddOwnerResult> HandleAsync(AddOwnerCommand command)
    {
        // بررسی مدیر بودن
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.RequestingManagerId)
            return new AddOwnerResult(false, "فقط مدیر ساختمان می‌تواند مالک اضافه کند");

        // پیدا کردن کاربر با شماره موبایل
        var owner = await _userRepository.GetByPhoneNumberAsync(command.OwnerPhoneNumber);
        if (owner is null)
            return new AddOwnerResult(false, "کاربری با این شماره موبایل یافت نشد. لطفاً ابتدا در سیستم ثبت‌نام کند");

        // چک کردن وجود واحد
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);

        if (unit is null)
        {
            // واحد جدید بساز
            unit = Unit.Create(command.BuildingId, command.Block, command.Floor, command.UnitNumber);
            await _unitRepository.AddAsync(unit);
            await _unitRepository.SaveChangesAsync();
        }
        else
        {
            // membership قبلی مالک رو deactivate کن
            var existingMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);
            foreach (var m in existingMemberships.Where(m => m.Role == UserRole.Owner && m.IsActive))
            {
                m.Deactivate();
                _membershipRepository.Update(m);
            }
            await _membershipRepository.SaveChangesAsync();
        }

        // membership جدید برای مالک بساز
        var membership = BuildingMembership.Create(
            owner.Id, command.BuildingId, unit.Id,
            UserRole.Owner, command.IsResident, DateTime.UtcNow);

        await _membershipRepository.AddAsync(membership);
        await _membershipRepository.SaveChangesAsync();

        return new AddOwnerResult(true, "مالک با موفقیت اضافه شد", unit.Id);
    }
}
