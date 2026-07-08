using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Reports.Commands.UpdateRepairStatus;

public record UpdateRepairStatusCommand(
    Guid ReportId,
    Guid ManagerUserId,
    RepairStatus Status);

public record UpdateRepairStatusResult(bool Success,string Message);

public class UpdateRepairStatusHandler
{
    private readonly IRepairReportRepository _repairRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public UpdateRepairStatusHandler(
        IRepairReportRepository repairRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _repairRepository = repairRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<UpdateRepairStatusResult> HandleAsync(UpdateRepairStatusCommand command)
    {
        var report =
            await _repairRepository.GetByIdAsync(command.ReportId);

        if (report is null)
            return new(false,"گزارش پیدا نشد.");

        var managerId =
            await _membershipRepository.GetCurrentManagerIdAsync(report.BuildingId);

        if(managerId!=command.ManagerUserId)
            return new(false,"فقط مدیر ساختمان مجاز است.");

        report.UpdateStatus(command.Status);

        await _repairRepository.SaveChangesAsync();

        return new(true,"وضعیت بروزرسانی شد.");
    }
}