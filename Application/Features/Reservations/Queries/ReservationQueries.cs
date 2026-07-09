using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Reservations.Queries;

public record GetReservedDatesQuery(
    Guid BuildingId,
    FacilityType FacilityType);

public class GetReservedDatesHandler
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IBuildingRepository _buildingRepository;

    public GetReservedDatesHandler(
        IReservationRepository reservationRepository,
        IBuildingRepository buildingRepository)
    {
        _reservationRepository = reservationRepository;
        _buildingRepository = buildingRepository;
    }

    public async Task<IEnumerable<DateOnly>> HandleAsync(GetReservedDatesQuery query)
    {
        var building = await _buildingRepository.GetByIdAsync(query.BuildingId);
        if (building is null)
            return Enumerable.Empty<DateOnly>();

        bool hasFacility = query.FacilityType switch
        {
            FacilityType.Gym => building.HasGym,
            FacilityType.Pool => building.HasPool,
            FacilityType.MeetingHall => building.HasMeetingHall,
            FacilityType.RoofGarden => building.HasRoofGarden,
            _ => false
        };

        if (!hasFacility)
            return Enumerable.Empty<DateOnly>();

        var reservations = await _reservationRepository
            .GetByBuildingAndFacilityAsync(query.BuildingId, query.FacilityType);

        return reservations
            .Select(r => DateOnly.FromDateTime(r.Date))
            .OrderBy(d => d);
    }
}

public record GetMyReservationsQuery(Guid UserId);

public record MyReservationItem(
    Guid Id,
    string BuildingName,
    FacilityType FacilityType,
    DateTime Date);

public class GetMyReservationsHandler
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IBuildingRepository _buildingRepository;

    public GetMyReservationsHandler(
        IReservationRepository reservationRepository,
        IBuildingRepository buildingRepository)
    {
        _reservationRepository = reservationRepository;
        _buildingRepository = buildingRepository;
    }

    public async Task<IEnumerable<MyReservationItem>> HandleAsync(GetMyReservationsQuery query)
    {
        var reservations = await _reservationRepository.GetByUserIdAsync(query.UserId);

        var result = new List<MyReservationItem>();

        foreach (var reservation in reservations)
        {
            var building = await _buildingRepository.GetByIdAsync(reservation.BuildingId);

            result.Add(new MyReservationItem(
                reservation.Id,
                building?.Name ?? "",
                reservation.FacilityType,
                reservation.Date));
        }

        return result;
    }
}