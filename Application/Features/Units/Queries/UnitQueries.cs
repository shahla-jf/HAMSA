using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Queries;

// ======================================================
// GetUnitsByBuilding — لیست واحدها برای مدیر
// ======================================================
public record GetUnitsByBuildingQuery(Guid BuildingId, Guid RequestingManagerId);

public record UnitMemberInfo(
    string FullName, 
    string PhoneNumber, 
    UserRole Role, 
    bool IsActive, 
    DateTime StartDate, 
    DateTime? EndDate);

// رکورد جدید برای وضعیت شارژ و پرداخت
public record UnitChargeStatus(
    int Year,
    int Month,
    decimal Amount,
    decimal PenaltyAmount,
    bool IsPaid,
    string? PendingTrackingCode,    // کد پیگیری اگر پرداخت در انتظار تایید باشد
    decimal? PendingAmount);        // مبلغ در انتظار تایید

public record UnitDetail(
    Guid UnitId, 
    int Block, 
    int Floor, 
    int UnitNumber, 
    IEnumerable<UnitMemberInfo> Members,
    UnitChargeStatus? CurrentChargeStatus);  // وضعیت شارژ ماه جاری

public class GetUnitsByBuildingHandler(
    IBuildingMembershipRepository membershipRepository,
    IUnitRepository unitRepository,
    IChargeRepository chargeRepository,
    ITransactionRepository transactionRepository)
{
    public async Task<IEnumerable<UnitDetail>> HandleAsync(GetUnitsByBuildingQuery query)
    {
        var managerId = await membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        if (managerId != query.RequestingManagerId)
            return Enumerable.Empty<UnitDetail>();

        // ۱. دریافت همه واحدهای ساختمان
        var units = await unitRepository.GetByBuildingIdAsync(query.BuildingId);
        if (!units.Any())
            return Enumerable.Empty<UnitDetail>();

        var unitIds = units.Select(u => u.Id).ToList();

        // ۲. دریافت همه اعضای واحدها (یکجا برای جلوگیری از N+1)
        var allMemberships = await membershipRepository.GetByUnitIdsAsync(unitIds);

        // ۳. دریافت شارژهای ماه جاری برای همه واحدها
        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;
        var charges = await chargeRepository.GetByUnitIdsAndMonthAsync(unitIds, currentYear, currentMonth);

        // ۴. دریافت تراکنش‌های در انتظار تایید برای این شارژها
        var chargeIds = charges.Select(c => c.Id).ToList();
        var pendingTransactions = await transactionRepository.GetPendingByChargeIdsAsync(chargeIds);

        // ۵. ساخت نتیجه
        var result = new List<UnitDetail>();

        foreach (var unit in units)
        {
            // اعضای این واحد
            var unitMembers = allMemberships
                .Where(m => m.UnitId == unit.Id)
                .Select(m => new UnitMemberInfo(
                    $"{m.User.FirstName} {m.User.LastName}",
                    m.User.PhoneNumber ?? string.Empty,
                    m.Role,
                    m.IsActive,
                    m.StartDate,
                    m.EndDate
                )).ToList();

            // شارژ ماه جاری این واحد
            var charge = charges.FirstOrDefault(c => c.UnitId == unit.Id);
            
            UnitChargeStatus? chargeStatus = null;
            if (charge is not null)
            {
                // تراکنش در انتظار تایید برای این شارژ
                var pendingTx = pendingTransactions.FirstOrDefault(t => t.ChargeId == charge.Id);
                
                chargeStatus = new UnitChargeStatus(
                    charge.Year,
                    charge.Month,
                    charge.Amount,
                    charge.PenaltyAmount,
                    charge.IsPaid,
                    pendingTx?.TrackingCode,
                    pendingTx?.Amount);
            }

            result.Add(new UnitDetail(
                unit.Id,
                unit.Block,
                unit.Floor,
                unit.UnitNumber,
                unitMembers,
                chargeStatus));
        }

        return result;
    }
}

// ======================================================
// GetMyTenants — لیست مستاجرین یک مالک
// ======================================================
public record GetMyTenantsQuery(Guid BuildingId, Guid OwnerUserId);

public record TenantInfo(
    string FullName, string PhoneNumber,
    DateTime StartDate, DateTime? EndDate,
    bool IsActive, int Block, int Floor, int UnitNumber);

public class GetMyTenantsHandler(IBuildingMembershipRepository membershipRepository)
{
    public async Task<IEnumerable<TenantInfo>> HandleAsync(GetMyTenantsQuery query)
    {
        var tenantMemberships = await membershipRepository.GetTenantsByOwnerAsync(
            query.OwnerUserId, query.BuildingId);

        // اضافه کردن ToList در انتهای Select
        return tenantMemberships.Select(m => new TenantInfo(
            $"{m.User.FirstName} {m.User.LastName}",
            m.User.PhoneNumber ?? string.Empty,
            m.StartDate,
            m.EndDate,
            m.IsActive,
            m.Unit!.Block,
            m.Unit!.Floor,
            m.Unit!.UnitNumber
        )).ToList();
    }
}

// ======================================================
// GetPrimaryOwners — لیست مالکین اصلی ساختمان و سابقه مدیریت آن‌ها
// ======================================================
public record GetPrimaryOwnersQuery(Guid BuildingId, Guid ManagerId);

public record PrimaryOwnerInfo(
    string FullName, string PhoneNumber, int Block, int Floor, int UnitNumber, int ManagementTermsCount);

public record PrimaryOwnersResult(bool Success, string Message, IEnumerable<PrimaryOwnerInfo>? PrimaryOwners);

public class GetPrimaryOwnersHandler(
    IBuildingMembershipRepository membershipRepository,
    IBuildingManagerHistoryRepository managerHistoryRepository)
{
    public async Task<PrimaryOwnersResult> HandleAsync(GetPrimaryOwnersQuery query)
    {
        var managerId = await membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        if (managerId != query.ManagerId)
            return new PrimaryOwnersResult(false, "فقط مدیر ساختمان می‌تواند لیست را مشاهده کند", null);

        var primaryOwners = await membershipRepository.GetPrimaryOwnersByBuildingAsync(query.BuildingId);
        var managerHistory = await managerHistoryRepository.GetHistoryAsync(query.BuildingId);
        
        var managementCounts = managerHistory
            .GroupBy(h => h.UserId)
            .ToDictionary(g => g.Key, g => g.Count());

        var listOwners = primaryOwners.Select(membership => new PrimaryOwnerInfo(
            FullName: $"{membership.User.FirstName} {membership.User.LastName}",
            PhoneNumber: membership.User.PhoneNumber ?? string.Empty,
            Block: membership.Unit?.Block ?? 0,
            Floor: membership.Unit?.Floor ?? 0,
            UnitNumber: membership.Unit?.UnitNumber ?? 0,
            ManagementTermsCount: managementCounts.GetValueOrDefault(membership.UserId, 0)
        )).ToList();
        
        return new PrimaryOwnersResult(true, "لیست افراد با موفقیت پیدا شد", listOwners);
    }
}

// ======================================================
// GetCoMembers — لیست اعضای واحدهای مشترک یک فرد در یک ساختمان
// ======================================================
public record GetCoMembersQuery(Guid BuildingId, Guid UserId);

public record CoMemberInfo(string FullName, string PhoneNumber, int Block, int Floor, int UnitNumber);
public record CoMembersResult(bool Success, string Message, IEnumerable<CoMemberInfo>? CoMemberInfos);

public class GetCoMembersHandler(IBuildingMembershipRepository membershipRepository)
{
    public async Task<CoMembersResult> HandleAsync(GetCoMembersQuery query)
    {
        var coMembers = await membershipRepository.GetCoMembersByUserInBuildingAsync(query.UserId, query.BuildingId);

        var result = coMembers.Select(m => new CoMemberInfo(
            FullName: $"{m.User.FirstName} {m.User.LastName}",
            PhoneNumber: m.User.PhoneNumber ?? string.Empty,
            Block: m.Unit?.Block ?? 0,
            Floor: m.Unit?.Floor ?? 0,
            UnitNumber: m.Unit?.UnitNumber ?? 0
        )).ToList();
        
        return new CoMembersResult(true, "لیست با موفقیت پیدا شد", result);
    }
}

// ======================================================
// GetUserUnits — لیست واحدهایی که کاربر در یک ساختمان عضو آن‌هاست
// ======================================================
public record GetUserUnitsQuery(Guid BuildingId, Guid UserId);

public record UserUnitDetail(Guid UnitId, int Block, int Floor, int UnitNumber, UserRole Role, bool IsPrimary);
public record UserUnitResult(bool Success, string Message, IEnumerable<UserUnitDetail>? UserUnitDetails);

public class GetUserUnitsHandler(IBuildingMembershipRepository membershipRepository)
{
    public async Task<UserUnitResult> HandleAsync(GetUserUnitsQuery query)
    {
        var memberships = await membershipRepository.GetUserUnitsInBuildingAsync(query.UserId, query.BuildingId);

        var unitList = memberships
            .Where(m => m.Unit != null)
            .Select(m => new UserUnitDetail(
                UnitId: m.Unit!.Id,
                Block: m.Unit.Block,
                Floor: m.Unit.Floor,
                UnitNumber: m.Unit.UnitNumber,
                Role: m.Role,
                IsPrimary: m.IsPrimary
            )).ToList();
        
        return new UserUnitResult(true, $"تعداد {unitList.Count} واحد یافت شد", unitList);
    }
}