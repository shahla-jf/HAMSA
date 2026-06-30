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
            var memberships = await _membershipRepository.GetByUnitIdAsync(unit.Id);
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
