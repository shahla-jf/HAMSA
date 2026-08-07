using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.AddOwner;

// ------- Command -------
public record AddOwnerCommand(
    Guid BuildingId,
    Guid RequestingUserId, // می‌تونه مدیر ساختمان باشه یا یه مالک فعال همون واحد
    string OwnerPhoneNumber,
    int Block,
    int Floor,
    int UnitNumber
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
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        var isManager = managerId == command.RequestingUserId;

        // چک کردن وجود واحد
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);

        List<BuildingMembership> unitMemberships = new();
        bool isOwnerOfUnit = false;

        if (unit is not null)
        {
            unitMemberships = (await _membershipRepository.GetByUnitIdAsync(unit.Id)).ToList();
            isOwnerOfUnit = unitMemberships.Any(m =>
                m.UserId == command.RequestingUserId &&
                m.Role == UserRole.Owner &&
                m.IsActive);
        }

        // اگه واحد وجود نداره، فقط مدیر می‌تونه واحد جدید بسازه و اولین مالکش رو اضافه کنه
        if (unit is null && !isManager)
            return new AddOwnerResult(false, "فقط مدیر ساختمان می‌تواند برای واحد جدید مالک اضافه کند");

        // اگه واحد وجود داره، یا باید مدیر باشه یا یکی از مالک‌های فعال همون واحد
        if (unit is not null && !isManager && !isOwnerOfUnit)
            return new AddOwnerResult(false, "فقط مدیر ساختمان یا مالک این واحد می‌تواند مالک اضافه کند");

        // پیدا کردن کاربر با شماره موبایل
        var owner = await _userRepository.GetByPhoneNumberAsync(command.OwnerPhoneNumber);
        if (owner is null)
            return new AddOwnerResult(false, "کاربری با این شماره موبایل یافت نشد. لطفاً ابتدا در سیستم ثبت‌نام کند");

        // اگه واحد نبود بسازش
        if (unit is null)
        {
            unit = Unit.Create(command.BuildingId, command.Block, command.Floor, command.UnitNumber);
            await _unitRepository.AddAsync(unit);
            await _unitRepository.SaveChangesAsync();
        }
        else
        {
            // چک کردن اینکه همین کاربر از قبل عضو فعال همین واحد نباشه (جلوگیری از duplicate)
            var alreadyMember = unitMemberships.Any(m =>
                m.UserId == owner.Id && m.IsActive);

            if (alreadyMember)
                return new AddOwnerResult(false, "این کاربر از قبل عضو فعال این واحد است");
        }

        // مالکی که توسط مدیر اضافه بشه اصلی است، مالکی که توسط مالک دیگه اضافه بشه فرعی است
        var isPrimary = isManager;

        var membership = BuildingMembership.Create(
            owner.Id, command.BuildingId, unit.Id,
            UserRole.Owner, DateTime.UtcNow, isPrimary, null);

        await _membershipRepository.AddAsync(membership);
        await _membershipRepository.SaveChangesAsync();

        return new AddOwnerResult(true, "مالک با موفقیت اضافه شد", unit.Id);
    }
}