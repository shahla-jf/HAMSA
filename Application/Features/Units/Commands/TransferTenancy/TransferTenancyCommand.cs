using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Commands.TransferTenancy;

// ------- Command -------
public record TransferTenancyCommand(
    Guid BuildingId,
    Guid RequestingOwnerId,
    string NewTenantPhoneNumber,
    int Block,
    int Floor,
    int UnitNumber,
    DateTime StartDate
);

// ------- Result -------
public record TransferTenancyResult(bool Success, string Message, string? InviteCode = null, int RemovedTenantsCount = 0);

// ------- Handler -------
public class TransferTenancyHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;

    public TransferTenancyHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository,
        IUserRepository userRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
        _userRepository = userRepository;
    }

    public async Task<TransferTenancyResult> HandleAsync(TransferTenancyCommand command)
    {
        // پیدا کردن واحد
        var unit = await _unitRepository.GetByBlockFloorUnitAsync(
            command.BuildingId, command.Block, command.Floor, command.UnitNumber);
        if (unit is null)
            return new TransferTenancyResult(false, "واحدی با این مشخصات یافت نشد");

        var unitMemberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);

        // بررسی مالک بودن درخواست‌دهنده برای همین واحد
        var isOwner = unitMemberships.Any(m =>
            m.UserId == command.RequestingOwnerId &&
            m.Role == UserRole.Owner &&
            m.IsActive);

        if (!isOwner)
            return new TransferTenancyResult(false, "شما مالک این واحد نیستید");

        // پیدا کردن مستاجر جدید
        var newTenant = await _userRepository.GetByPhoneNumberAsync(command.NewTenantPhoneNumber);
        if (newTenant is null)
            return new TransferTenancyResult(false, "کاربری با این شماره موبایل یافت نشد. لطفاً ابتدا در سیستم ثبت‌نام کند");

        // همه‌ی مستاجرین فعلی این واحد رو deactivate کن
        var currentTenants = unitMemberships.Where(m => m.Role == UserRole.Tenant && m.IsActive).ToList();
        foreach (var m in currentTenants)
        {
            m.Deactivate();
            _membershipRepository.Update(m);
        }
        await _membershipRepository.SaveChangesAsync();

        // مستاجر جدید رو اضافه کن
        var membership = BuildingMembership.Create(
            newTenant.Id, command.BuildingId, unit.Id,
            UserRole.Tenant, true, command.StartDate);

        membership.GenerateInviteCode();

        await _membershipRepository.AddAsync(membership);
        await _membershipRepository.SaveChangesAsync();

        return new TransferTenancyResult(true, "اجاره واحد با موفقیت منتقل شد", membership.InviteCode, currentTenants.Count);
    }
}