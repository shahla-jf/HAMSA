using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

// آگهی همسایه‌ها (خرید و فروش وسایل / وسایل امانی)
public class Listing
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ListingType Type { get; private set; }
    public decimal? Price { get; private set; }
    public string ContactPhone { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public bool IsSold { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Building Building { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    private Listing() { }

    public static Listing Create(
        Guid buildingId, Guid createdByUserId,
        string title, string description, ListingType type,
        decimal? price, string contactPhone, string? imageUrl)
    {
        return new Listing
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            CreatedByUserId = createdByUserId,
            Title = title,
            Description = description,
            Type = type,
            Price = price,
            ContactPhone = contactPhone,
            ImageUrl = imageUrl,
            IsSold = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string description, ListingType type,
        decimal? price, string contactPhone, string? imageUrl)
    {
        Title = title;
        Description = description;
        Type = type;
        Price = price;
        ContactPhone = contactPhone;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSold()
    {
        IsSold = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

// رویداد / کلاس ساکنین
public class ResidentEvent
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string OrganizerFullName { get; private set; } = string.Empty;
    public string EventTime { get; private set; }
    public EventCategory Category { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ContactPhone { get; private set; } = string.Empty;
    public int Block { get; private set; }
    public int Floor { get; private set; }
    public int UnitNumber { get; private set; }
    public decimal RegistrationFee { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Building Building { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;


    public IReadOnlyCollection<EventRegistration> Registrations => _registrations.AsReadOnly();
    private readonly List<EventRegistration> _registrations = new();

    private ResidentEvent() { }

    public static ResidentEvent Create(
        Guid buildingId, Guid createdByUserId, string organizerFullName, string eventTime,
        EventCategory category, string title, string description,
        string contactPhone, int block, int floor, int unitNumber, decimal registrationFee)
    {
        return new ResidentEvent
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            CreatedByUserId = createdByUserId,
            OrganizerFullName = organizerFullName,
            EventTime = eventTime,
            Category = category,
            Title = title,
            Description = description,
            ContactPhone = contactPhone,
            Block = block,
            Floor = floor,
            UnitNumber = unitNumber,
            RegistrationFee = registrationFee,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddRegistration(Guid userId)
    {
        if (_registrations.Any(r => r.UserId == userId))
            throw new InvalidOperationException("شما قبلاً در این رویداد ثبت‌نام کرده‌اید.");

        _registrations.Add(EventRegistration.Create(Id, userId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRegistration(Guid userId)
    {
        var registration = _registrations.FirstOrDefault(r => r.UserId == userId);
        if (registration is not null)
        {
            _registrations.Remove(registration);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool IsOrganizer(Guid userId) => CreatedByUserId == userId;
}

// ثبت‌نام در رویداد
public class EventRegistration
{
    public Guid Id { get; private set; }
    public Guid ResidentEventId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    public ResidentEvent ResidentEvent { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private EventRegistration() { }

    public static EventRegistration Create(Guid residentEventId, Guid userId)
    {
        return new EventRegistration
        {
            Id = Guid.NewGuid(),
            ResidentEventId = residentEventId,
            UserId = userId,
            RegisteredAt = DateTime.UtcNow
        };
    }
}
