using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.AddTenant;

// ------- Command -------
public record AddTenantCommand(
    Guid BuildingId,
    Guid RequestingOwnerId,
    string TenantPhoneNumber,
    int Block,
    int Floor,
    int UnitNumber,
    DateTime StartDate
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
        // پیدا کردن واحد
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);
        if (unit is null)
            return new AddTenantResult(false, "واحد مورد نظر یافت نشد");

        // بررسی مالک بودن درخواست‌دهنده برای این واحد
        var ownerMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);
        var isOwner = ownerMemberships.Any(m =>
            m.UserId == command.RequestingOwnerId &&
            m.Role == UserRole.Owner &&
            m.IsActive);

        if (!isOwner)
            return new AddTenantResult(false, "شما مالک این واحد نیستید");

        // پیدا کردن مستاجر با شماره موبایل
        var tenant = await _userRepository.GetByPhoneNumberAsync(command.TenantPhoneNumber);
        if (tenant is null)
            return new AddTenantResult(false, "کاربری با این شماره موبایل یافت نشد. لطفاً ابتدا در سیستم ثبت‌نام کند");

        // مستاجر قبلی رو deactivate کن
        foreach (var m in ownerMemberships.Where(m => m.Role == UserRole.Tenant && m.IsActive))
        {
            m.Deactivate();
            _membershipRepository.Update(m);
        }
        await _membershipRepository.SaveChangesAsync();

        // membership جدید برای مستاجر بساز
        var membership = BuildingMembership.Create(
            tenant.Id, command.BuildingId, unit.Id,
            UserRole.Tenant, true, command.StartDate);

        // کد دعوت بساز
        membership.GenerateInviteCode();

        await _membershipRepository.AddAsync(membership);
        await _membershipRepository.SaveChangesAsync();

        return new AddTenantResult(true, "مستاجر با موفقیت اضافه شد", membership.InviteCode);
    }
}
