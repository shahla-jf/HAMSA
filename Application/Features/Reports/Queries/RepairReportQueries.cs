using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Reports.Queries;

public record GetRepairReportsQuery(
    Guid BuildingId,
    Guid ManagerUserId);

public record RepairReportItem(
    Guid Id,
    string Reporter,
    string Title,
    string Description,
    RepairPriority Priority,
    RepairStatus Status,
    DateTime CreatedAt);

public class GetRepairReportsHandler
{
    private readonly IRepairReportRepository _repairRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetRepairReportsHandler(
        IRepairReportRepository repairRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _repairRepository = repairRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<RepairReportItem>> HandleAsync(GetRepairReportsQuery query)
    {
        var managerId =
            await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);

        if(managerId!=query.ManagerUserId)
            return Enumerable.Empty<RepairReportItem>();

        var reports =
            await _repairRepository.GetByBuildingIdAsync(query.BuildingId);

        return reports.Select(r =>
            new RepairReportItem(
                r.Id,
                $"{r.ReportedByUser.FirstName} {r.ReportedByUser.LastName}".Trim(),
                r.Title,
                r.Description,
                r.Priority,
                r.Status,
                r.CreatedAt));
    }
}