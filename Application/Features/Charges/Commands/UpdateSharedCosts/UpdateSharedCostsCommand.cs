using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Charges.Commands.UpdateSharedCosts;

// ------- Command -------
public record UpdateSharedCostsCommand(
    Guid BuildingId,
    Guid RequestingManagerId,
    decimal Electricity,
    decimal Water,
    decimal Cleaning,
    decimal Elevator
);

// ------- Result -------
public record UpdateSharedCostsResult(bool Success, string Message);

// ------- Handler -------
public class UpdateSharedCostsHandler
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public UpdateSharedCostsHandler(
        IBuildingRepository buildingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _buildingRepository = buildingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<UpdateSharedCostsResult> HandleAsync(UpdateSharedCostsCommand command)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.RequestingManagerId)
            return new UpdateSharedCostsResult(false, "فقط مدیر ساختمان می‌تواند هزینه‌های مشاعات را ویرایش کند");

        var building = await _buildingRepository.GetByIdAsync(command.BuildingId);
        if (building is null)
            return new UpdateSharedCostsResult(false, "ساختمان یافت نشد");

        building.UpdateSharedCosts(command.Electricity, command.Water, command.Cleaning, command.Elevator);
        _buildingRepository.Update(building);
        await _buildingRepository.SaveChangesAsync();

        return new UpdateSharedCostsResult(true, "هزینه‌های مشاعات بروزرسانی شد");
    }
}
