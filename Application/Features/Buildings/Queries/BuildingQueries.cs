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
            building.ImageUrl, building.SharedElectricityCost, building.SharedWaterCost,
            building.CleaningCost, building.ElevatorCost, isManager
        );
    }
}

/// <summary>
/// GetBuildingImage
/// </summary>
public record GetBuildingImageQuery(Guid UserId, Guid BuildingId);

public record GetBuildingImageResult(bool Success, string Message, string? ImageUrl);

public class GetBuildingImageHandler
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _buildingMembershipRepository;

    public GetBuildingImageHandler(IBuildingRepository buildingRepository,
        IBuildingMembershipRepository buildingMembershipRepository)
    {
        _buildingRepository = buildingRepository;
        _buildingMembershipRepository = buildingMembershipRepository;
    }

    public async Task<GetBuildingImageResult> HandleAsync(GetBuildingImageQuery query)
    {
        var building = await _buildingRepository.GetByIdAsync(query.BuildingId);
        if (building is null) return new(false, "ساختمان پیدا نشد", null);
        
        var membership = await _buildingMembershipRepository.GetActiveAsync(query.UserId, query.BuildingId);
        if (membership is null) return new(false, "شما عضو این ساختمان نیستید", null);

        if (string.IsNullOrEmpty(building.ImageUrl))
            return new(false, "برای این ساختمان هنوز عکسی وجود ندارد", null);
        return new(true, "عکس با موفقیت یافت شد", building.ImageUrl);
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
// ------- Query -------
public record GetMyRoleInBuildingQuery(Guid UserId, Guid BuildingId);

// ------- Result -------
public record GetMyRoleInBuildingResult(
    bool Success,
    string Message,
    bool IsManager,
    UserRole? Role   // نقش عضویتی (Owner/Tenant) - مستقل از IsManager
);

// ------- Handler -------
public class GetMyRoleInBuildingHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetMyRoleInBuildingHandler(IBuildingMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<GetMyRoleInBuildingResult> HandleAsync(GetMyRoleInBuildingQuery query)
    {
        // بررسی مدیر بودن (جدا از membership چک می‌شه)
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(query.BuildingId);
        var isManager = managerId == query.UserId;

        if (isManager)
        {
            return new GetMyRoleInBuildingResult(
                true,
                "عملیات موفق بود.",
                true,
                UserRole.Manager
            );
        }

        var membership = await _membershipRepository.GetActiveAsync(query.UserId, query.BuildingId);

        if (membership is null)
        {
            return new(false, "کاربر عضو این ساختمان نیست.", false, null);
        }

        return new(
            true,
            "عملیات موفق بود.",
            false,
            membership?.Role);
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