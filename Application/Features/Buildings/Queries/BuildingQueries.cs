using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Buildings.Queries;

// ======================================================
// GetBuilding
// ======================================================
public record GetBuildingQuery(Guid BuildingId, Guid RequestingUserId);

public record GetBuildingResult(
    Guid Id,
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
    string FacilitiesPhone,
    string ManagementPhone,
    string LobbyPhone,
    string? ImageUrl,
    decimal SharedElectricityCost,
    decimal SharedWaterCost,
    decimal CleaningCost,
    decimal ElevatorCost,
    bool IsManager
);

public class GetBuildingHandler
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetBuildingHandler(
        IBuildingRepository buildingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _buildingRepository = buildingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<GetBuildingResult?> HandleAsync(GetBuildingQuery query)
    {
        var building = await _buildingRepository.GetByIdAsync(query.BuildingId);
        if (building is null) return null;

        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        var isManager = managerId == query.RequestingUserId;

        return new GetBuildingResult(
            building.Id, building.Name, building.BlockCount, building.FloorCount,
            building.UnitCount, building.PostalCode, building.Address,
            building.Latitude, building.Longitude,
            building.HasGym, building.HasPool, building.HasMeetingHall, building.HasRoofGarden,
            building.FacilitiesPhone, building.ManagementPhone, building.LobbyPhone,
            building.ImageUrl, building.SharedElectricityCost, building.SharedWaterCost,
            building.CleaningCost, building.ElevatorCost, isManager
        );
    }
}

// ======================================================
// GetUserBuildings
// ======================================================
public record GetUserBuildingsQuery(Guid UserId);

public record BuildingSummary(Guid Id, string Name, string? ImageUrl, string UserRole);

public class GetUserBuildingsHandler
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetUserBuildingsHandler(
        IBuildingRepository buildingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _buildingRepository = buildingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<BuildingSummary>> HandleAsync(GetUserBuildingsQuery query)
    {
        var buildings = await _buildingRepository.GetBuildingsByUserIdAsync(query.UserId);

        var result = new List<BuildingSummary>();
        foreach (var building in buildings)
        {
            var membership = await _membershipRepository.GetActiveAsync(query.UserId, building.Id);
            var role = membership?.Role.ToString() ?? "Unknown";
            result.Add(new BuildingSummary(building.Id, building.Name, building.ImageUrl, role));
        }

        return result;
    }
}
