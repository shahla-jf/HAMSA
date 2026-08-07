using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Units.Queries;

// ======================================================
// GetUnitsByBuilding — لیست واحدها برای مدیر
// ======================================================
public record GetUnitsByBuildingQuery(Guid BuildingId, Guid RequestingManagerId);

public record UnitMemberInfo(string FullName, string PhoneNumber, UserRole Role, bool IsActive, DateTime StartDate, DateTime? EndDate);
public record UnitDetail(Guid UnitId, int Block, int Floor, int UnitNumber, IEnumerable<UnitMemberInfo> Members);

public class GetUnitsByBuildingHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUnitRepository _unitRepository;

    public GetUnitsByBuildingHandler(
        IBuildingMembershipRepository membershipRepository,
        IUnitRepository unitRepository)
    {
        _membershipRepository = membershipRepository;
        _unitRepository = unitRepository;
    }

    public async Task<IEnumerable<UnitDetail>> HandleAsync(GetUnitsByBuildingQuery query)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        if (managerId != query.RequestingManagerId)
            return Enumerable.Empty<UnitDetail>();

        var units = await _unitRepository.GetByBuildingIdAsync(query.BuildingId);

        var result = new List<UnitDetail>();
        foreach (var unit in units)
        {
            var memberships = await _membershipRepository.GetAllByUnitIdAsync(unit.Id);
            var members = memberships.Select(m => new UnitMemberInfo(
                $"{m.User.FirstName} {m.User.LastName}",
                m.User.PhoneNumber,
                m.Role,
                m.IsActive,
                m.StartDate,
                m.EndDate
            ));
            result.Add(new UnitDetail(unit.Id, unit.Block, unit.Floor, unit.UnitNumber, members));
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

public class GetMyTenantsHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetMyTenantsHandler(IBuildingMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<TenantInfo>> HandleAsync(GetMyTenantsQuery query)
    {
        var tenantMemberships = await _membershipRepository.GetTenantsByOwnerAsync(
            query.OwnerUserId, query.BuildingId);

        return tenantMemberships.Select(m => new TenantInfo(
            $"{m.User.FirstName} {m.User.LastName}",
            m.User.PhoneNumber,
            m.StartDate,
            m.EndDate,
            m.IsActive,
            m.Unit!.Block,
            m.Unit!.Floor,
            m.Unit!.UnitNumber
        ));
    }
}


// ======================================================
// GetPrimaryOwners — لیست مالکین اصلی ساختمان و سابقه مدیریت آن‌ها
// ======================================================
public record GetPrimaryOwnersQuery(Guid BuildingId, Guid ManagerId);

public record PrimaryOwnerInfo(
    string FullName, 
    string PhoneNumber, 
    int Block, 
    int Floor, 
    int UnitNumber, 
    int ManagementTermsCount);

public record PrimaryOwnersResult(bool Success, string Message , IEnumerable<PrimaryOwnerInfo>? PrimaryOwners);

public class GetPrimaryOwnersHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IBuildingManagerHistoryRepository _managerHistoryRepository;

    public GetPrimaryOwnersHandler(
        IBuildingMembershipRepository membershipRepository,
        IBuildingManagerHistoryRepository managerHistoryRepository)
    {
        _membershipRepository = membershipRepository;
        _managerHistoryRepository = managerHistoryRepository;
    }

    public async Task<PrimaryOwnersResult> HandleAsync(GetPrimaryOwnersQuery query)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        if (managerId != query.ManagerId)
            return new PrimaryOwnersResult(false, "فقط مدیر ساختمان می‌تواند لیست را مشاهده کند", null);

        
        // ۱. دریافت مالکین اصلی (Primary Owners)
        var primaryOwners = await _membershipRepository.GetPrimaryOwnersByBuildingAsync(query.BuildingId);
        
        // ۲. دریافت سابقه مدیریت‌ها برای شمارش دفعات مدیریت هر کاربر در این ساختمان
        var managerHistory = await _managerHistoryRepository.GetHistoryAsync(query.BuildingId);
        
        // گروه‌بندی بر اساس UserId و شمارش تعداد دفعات (بهینه‌تر از Count گرفتن در حلقه)
        var managementCounts = managerHistory
            .GroupBy(h => h.UserId)
            .ToDictionary(g => g.Key, g => g.Count());

        // ۳. ترکیب اطلاعات و ساخت خروجی نهایی
        var listOwners = primaryOwners.Select(membership => new PrimaryOwnerInfo(
            FullName: $"{membership.User.FirstName} {membership.User.LastName}",
            PhoneNumber: membership.User.PhoneNumber ?? string.Empty,
            Block: membership.Unit?.Block ?? 0,
            Floor: membership.Unit?.Floor ?? 0,
            UnitNumber: membership.Unit?.UnitNumber ?? 0,
            ManagementTermsCount: managementCounts.GetValueOrDefault(membership.UserId, 0)
        )).ToList();
        
        return new PrimaryOwnersResult(true, "لیست افراد با موفقیت پیدا شد",  listOwners);
    }
}


// ======================================================
// GetCoMembers — لیست اعضای واحدهای مشترک یک فرد در یک ساختمان
// ======================================================
public record GetCoMembersQuery(Guid BuildingId, Guid UserId);

public record CoMemberInfo(
    string FullName,
    string PhoneNumber,
    int Block,
    int Floor,
    int UnitNumber);

public record CoMembersResult(bool Success, string Message , IEnumerable<CoMemberInfo>? CoMemberInfos);

public class GetCoMembersHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetCoMembersHandler(IBuildingMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<CoMembersResult> HandleAsync(GetCoMembersQuery query)
    {
        var coMembers = await _membershipRepository.GetCoMembersByUserInBuildingAsync(
            query.UserId, query.BuildingId);

        var result = coMembers.Select(m => new CoMemberInfo(
            FullName: $"{m.User.FirstName} {m.User.LastName}",
            PhoneNumber: m.User.PhoneNumber ?? string.Empty,
            Block: m.Unit?.Block ?? 0,
            Floor: m.Unit?.Floor ?? 0,
            UnitNumber: m.Unit?.UnitNumber ?? 0
        ));
        
        return new CoMembersResult(true, "لیست با موفقیت پیدا شد",  result);
    }
}