using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Domain.Enums;

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


// ======================================================
// GetMyRoleInBuilding
// ======================================================
public record GetMyRoleInBuildingQuery(
    Guid UserId,
    Guid BuildingId);

public record GetMyRoleInBuildingResult(
    bool Success,
    string Message,
    UserRole? Role);
    


public class GetMyRoleInBuildingHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetMyRoleInBuildingHandler(
        IBuildingMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<GetMyRoleInBuildingResult> HandleAsync(
        GetMyRoleInBuildingQuery query)
    {
        var membership = await _membershipRepository
            .GetActiveAsync(query.UserId, query.BuildingId);

        if (membership is null)
        {
            return new(
                false,
                "کاربر عضو این ساختمان نیست.",
                null);
        }

        return new(
            true,
            "عملیات موفق بود.",
            membership.Role);
    }
}



// ======================================================
// GetLastSelectedBuilding
// ======================================================
public record GetLastSelectedBuildingQuery(Guid UserId);

public record LastSelectedBuildingDto(
    Guid BuildingId,
    string BuildingName);

public class GetLastSelectedBuildingHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetLastSelectedBuildingHandler(
        IBuildingMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<LastSelectedBuildingDto?> HandleAsync(
        GetLastSelectedBuildingQuery query)
    {
        var membership =
            await _membershipRepository.GetLastSelectedAsync(query.UserId);

        if (membership is null)
            return null;

        return new LastSelectedBuildingDto(
            membership.BuildingId,
            membership.Building.Name);
    }
}