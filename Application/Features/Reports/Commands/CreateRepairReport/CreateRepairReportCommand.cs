using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Reports.Commands.CreateRepairReport;

public record CreateRepairReportCommand(
    Guid BuildingId,
    Guid UserId,
    string Title,
    string Description,
    RepairPriority Priority);

public record CreateRepairReportResult(
    bool Success,
    string Message,
    Guid? ReportId = null);

public class CreateRepairReportHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IRepairReportRepository _repairRepository;

    public CreateRepairReportHandler(
        IBuildingMembershipRepository membershipRepository,
        IRepairReportRepository repairRepository)
    {
        _membershipRepository = membershipRepository;
        _repairRepository = repairRepository;
    }

    public async Task<CreateRepairReportResult> HandleAsync(CreateRepairReportCommand command)
    {
        var membership =
            await _membershipRepository.GetActiveAsync(command.UserId, command.BuildingId);

        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.");

        var report = RepairReport.Create(
            command.BuildingId,
            command.UserId,
            command.Title,
            command.Description,
            command.Priority);

        await _repairRepository.AddAsync(report);
        await _repairRepository.SaveChangesAsync();

        return new(true, "گزارش ثبت شد.", report.Id);
    }
}