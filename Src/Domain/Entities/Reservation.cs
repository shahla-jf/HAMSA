using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

// رزرو امکانات مشترک
public class Reservation
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid UserId { get; private set; }
    public FacilityType FacilityType { get; private set; }
    public DateTime Date { get; private set; }       // فقط روز (بدون ساعت)
    public DateTime CreatedAt { get; private set; }

    public Building Building { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private Reservation() { }

    public static Reservation Create(Guid buildingId, Guid userId, FacilityType facilityType, DateTime date)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            UserId = userId,
            FacilityType = facilityType,
            Date = date.Date,
            CreatedAt = DateTime.UtcNow
        };
    }
}
