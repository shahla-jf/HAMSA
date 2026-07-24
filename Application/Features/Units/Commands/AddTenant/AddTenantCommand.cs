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

        var unitMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);

        // بررسی مالک بودن درخواست‌دهنده برای این واحد
        var isOwner = unitMemberships.Any(m =>
            m.UserId == command.RequestingOwnerId &&
            m.Role == UserRole.Owner &&
            m.IsActive);

        if (!isOwner)
            return new AddTenantResult(false, "شما مالک این واحد نیستید");

        // پیدا کردن مستاجر با شماره موبایل
        var tenant = await _userRepository.GetByPhoneNumberAsync(command.TenantPhoneNumber);
        if (tenant is null)
            return new AddTenantResult(false, "کاربری با این شماره موبایل یافت نشد. لطفاً ابتدا در سیستم ثبت‌نام کند");

        // چک اینکه همین کاربر از قبل مستاجر فعال همین واحد نباشه (جلوگیری از duplicate)
        var alreadyTenant = unitMemberships.Any(m =>
            m.UserId == tenant.Id &&
            m.Role == UserRole.Tenant &&
            m.IsActive);

        if (alreadyTenant)
            return new AddTenantResult(false, "این کاربر از قبل مستاجر فعال این واحد است");

        // membership جدید برای مستاجر بساز — بدون دست‌زدن به مستاجرین قبلی
        var membership = BuildingMembership.Create(
            tenant.Id, command.BuildingId, unit.Id,
            UserRole.Tenant, true, command.StartDate);

        membership.GenerateInviteCode();

        await _membershipRepository.AddAsync(membership);
        await _membershipRepository.SaveChangesAsync();

        return new AddTenantResult(true, "مستاجر با موفقیت اضافه شد", membership.InviteCode);
    }
}