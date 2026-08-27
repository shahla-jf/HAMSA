using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Buildings.Commands.CreateBuilding;

// ------- Command -------
public record CreateBuildingCommand(
    Guid ManagerUserId,
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
    string? ImageUrl = null
);

// ------- Result -------
public record CreateBuildingResult(bool Success, string Message, Guid? BuildingId = null);

// ------- Handler -------
public class CreateBuildingHandler
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IBuildingManagerHistoryRepository _managerHistoryRepository;

    public CreateBuildingHandler(
        IBuildingRepository buildingRepository,
        IBuildingMembershipRepository membershipRepository,
        IBuildingManagerHistoryRepository managerHistoryRepository)
    {
        _buildingRepository = buildingRepository;
        _membershipRepository = membershipRepository;
        _managerHistoryRepository = managerHistoryRepository;
    }

    public async Task<CreateBuildingResult> HandleAsync(CreateBuildingCommand command)
    {
        // 1. ساختمان را بساز
        var building = Building.Create(
            command.Name, command.BlockCount, command.FloorCount, command.UnitCount,
            command.PostalCode, command.Address, command.Latitude, command.Longitude,
            command.HasGym, command.HasPool, command.HasMeetingHall, command.HasRoofGarden,
            command.FacilitiesPhone, command.ManagementPhone, command.LobbyPhone, command.ImageUrl
        );
        await _buildingRepository.AddAsync(building);
        
        // 2. سازنده را به عنوان مدیر ثبت کن
        var managerHistory = BuildingManagerHistory.Create(building.Id, command.ManagerUserId);
        await _managerHistoryRepository.AddAsync(managerHistory);
        
        // 3. سازنده را به عنوان عضو ساختمان (با نقش Manager) اضافه کن
        var membership = BuildingMembership.Create(
            command.ManagerUserId, building.Id,
            null,  // مدیر لزوماً واحد خاصی نداره
            UserRole.Manager, DateTime.UtcNow, true, null
        );

        // ساختمان را به عنوان آخرین ساختمان انتخاب‌شده علامت‌گذاری کن
        membership.MarkAsLastSelected();

        await _membershipRepository.AddAsync(membership);

        await _buildingRepository.SaveChangesAsync();
        await _managerHistoryRepository.SaveChangesAsync();
        await _membershipRepository.SaveChangesAsync();

        return new CreateBuildingResult(true, "ساختمان با موفقیت ایجاد و به عنوان ساختمان جاری تنظیم شد", building.Id);
    }
}
