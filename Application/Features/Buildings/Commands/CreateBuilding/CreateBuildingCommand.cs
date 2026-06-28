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

    public CreateBuildingHandler(
        IBuildingRepository buildingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _buildingRepository = buildingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<CreateBuildingResult> HandleAsync(CreateBuildingCommand command)
    {
        // ساختمان بساز
        var building = Building.Create(
            command.Name, command.BlockCount, command.FloorCount, command.UnitCount,
            command.PostalCode, command.Address, command.Latitude, command.Longitude,
            command.HasGym, command.HasPool, command.HasMeetingHall, command.HasRoofGarden,
            command.FacilitiesPhone, command.ManagementPhone, command.LobbyPhone, command.ImageUrl
        );
        await _buildingRepository.AddAsync(building);
        await _buildingRepository.SaveChangesAsync();

        // سازنده رو به عنوان مدیر ثبت کن
        var managerHistory = BuildingManagerHistory.Create(building.Id, command.ManagerUserId);
        // این رو باید از طریق IBuildingManagerHistoryRepository ذخیره کنیم
        // فعلاً از طریق DbContext مستقیم handle میشه - در ادامه Repository اضافه میشه

        // سازنده رو به عنوان عضو ساختمان (با نقش Manager) اضافه کن
        var membership = BuildingMembership.Create(
            command.ManagerUserId, building.Id,
            Guid.Empty,  // مدیر لزوماً واحد خاصی نداره
            UserRole.Manager, true, DateTime.UtcNow
        );
        await _membershipRepository.AddAsync(membership);
        await _membershipRepository.SaveChangesAsync();

        return new CreateBuildingResult(true, "ساختمان با موفقیت ایجاد شد", building.Id);
    }
}
