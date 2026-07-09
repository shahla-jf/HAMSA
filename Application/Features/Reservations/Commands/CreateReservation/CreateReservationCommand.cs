using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(
    Guid BuildingId,
    Guid UserId,
    FacilityType FacilityType,
    DateTime Date);

public record CreateReservationResult(
    bool Success,
    string Message,
    Guid? ReservationId = null);

public class CreateReservationHandler
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public CreateReservationHandler(
        IReservationRepository reservationRepository,
        IBuildingRepository buildingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _reservationRepository = reservationRepository;
        _buildingRepository = buildingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<CreateReservationResult> HandleAsync(CreateReservationCommand command)
    {
        var membership = await _membershipRepository.GetActiveAsync(command.UserId, command.BuildingId);

        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.");

        var building = await _buildingRepository.GetByIdAsync(command.BuildingId);

        if (building is null)
            return new(false, "ساختمان پیدا نشد.");

        bool hasFacility = command.FacilityType switch
        {
            FacilityType.Gym => building.HasGym,
            FacilityType.Pool => building.HasPool,
            FacilityType.MeetingHall => building.HasMeetingHall,
            FacilityType.RoofGarden => building.HasRoofGarden,
            _ => false
        };

        if (!hasFacility)
            return new(false, "این امکان در ساختمان وجود ندارد.");

        if (command.Date.Date < DateTime.Today)
            return new(false, "امکان رزرو تاریخ گذشته وجود ندارد.");

        if (await _reservationRepository.IsReservedAsync(
                command.BuildingId,
                command.FacilityType,
                command.Date))
            return new(false, "این تاریخ قبلا رزرو شده است.");

        var reservation = Reservation.Create(
            command.BuildingId,
            command.UserId,
            command.FacilityType,
            command.Date);

        await _reservationRepository.AddAsync(reservation);
        await _reservationRepository.SaveChangesAsync();

        return new(true, "رزرو با موفقیت انجام شد.", reservation.Id);
    }
}