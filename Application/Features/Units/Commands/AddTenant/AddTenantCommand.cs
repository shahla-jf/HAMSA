using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.AddTenant;

// ------- Command -------
public record AddTenantCommand(
    Guid BuildingId,
    Guid RequestingUserId, // می‌تونه مالک واحد باشه یا یه مستاجر فعال همون واحد
    string TenantPhoneNumber,
    int Block,
    int Floor,
    int UnitNumber
);

// ------- Result -------
public record AddTenantResult(bool Success, string Message, string? InviteCode = null);

// ------- Handler -------
public class AddTenantHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;

    public AddTenantHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository,
        IUserRepository userRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
        _userRepository = userRepository;
    }

    public async Task<AddTenantResult> HandleAsync(AddTenantCommand command)
    {
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);
        if (unit is null)
            return new AddTenantResult(false, "واحد مورد نظر یافت نشد");

        var unitMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);

        var isOwnerRequester = unitMemberships.Any(m =>
            m.UserId == command.RequestingUserId &&
            m.Role == UserRole.Owner &&
            m.IsActive);

        var isTenantRequester = unitMemberships.Any(m =>
            m.UserId == command.RequestingUserId &&
            m.Role == UserRole.Tenant &&
            m.IsActive);

        if (!isOwnerRequester && !isTenantRequester)
            return new AddTenantResult(false, "شما مالک یا مستاجر این واحد نیستید");

        var tenant = await _userRepository.GetByPhoneNumberAsync(command.TenantPhoneNumber);
        if (tenant is null)
            return new AddTenantResult(false, "کاربری با این شماره موبایل یافت نشد. لطفاً ابتدا در سیستم ثبت‌نام کند");

        var alreadyTenant = unitMemberships.Any(m =>
            m.UserId == tenant.Id &&
            m.Role == UserRole.Tenant &&
            m.IsActive);

        if (alreadyTenant)
            return new AddTenantResult(false, "این کاربر از قبل مستاجر فعال این واحد است");

        // مستاجری که توسط مالک اضافه بشه اصلی است، مستاجری که توسط مستاجر دیگه اضافه بشه فرعی است
        var isPrimary = isOwnerRequester;

        var membership = BuildingMembership.Create(
            tenant.Id, command.BuildingId, unit.Id,
            UserRole.Tenant, DateTime.UtcNow, isPrimary, DateTime.UtcNow.AddYears(1));

        membership.GenerateInviteCode();

        await _membershipRepository.AddAsync(membership);
        await _membershipRepository.SaveChangesAsync();

        return new AddTenantResult(true, "مستاجر با موفقیت اضافه شد", membership.InviteCode);
    }
}