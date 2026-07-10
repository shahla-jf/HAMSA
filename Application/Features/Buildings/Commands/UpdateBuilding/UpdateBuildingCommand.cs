using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Buildings.Commands.UpdateBuilding;

// ------- Command -------
public record UpdateBuildingCommand(
    Guid BuildingId,
    Guid RequestingUserId,
    string Name,
    int BlockCount,
    int FloorCount,
    int UnitCount,
    string PostalCode,
    string Address,
    double Latitude,
    double Longitude,
    bool HasGym,
    bool HasPool,
    bool HasMeetingHall,
    bool HasRoofGarden,
    string? FacilitiesPhone,
    string? ManagementPhone,
    string? LobbyPhone,
    string? ImageUrl = null
);

// ------- Result -------
public record UpdateBuildingResult(bool Success, string Message);

// ------- Handler -------
public class UpdateBuildingHandler
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public UpdateBuildingHandler(
        IBuildingRepository buildingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _buildingRepository = buildingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<UpdateBuildingResult> HandleAsync(UpdateBuildingCommand command)
    {
        // بررسی مدیر بودن
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.RequestingUserId)
            return new UpdateBuildingResult(false, "فقط مدیر ساختمان می‌تواند اطلاعات را ویرایش کند");

        var building = await _buildingRepository.GetByIdAsync(command.BuildingId);
        if (building is null)
            return new UpdateBuildingResult(false, "ساختمان یافت نشد");

        building.Update(
            command.Name, command.BlockCount, command.FloorCount, command.UnitCount,
            command.PostalCode, command.Address, command.Latitude, command.Longitude,
            command.HasGym, command.HasPool, command.HasMeetingHall, command.HasRoofGarden,
            command.FacilitiesPhone, command.ManagementPhone, command.LobbyPhone, command.ImageUrl
        );

        _buildingRepository.Update(building);
        await _buildingRepository.SaveChangesAsync();

        return new UpdateBuildingResult(true, "اطلاعات ساختمان با موفقیت بروزرسانی شد");
    }
}
