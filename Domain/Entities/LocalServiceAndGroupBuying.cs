using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

// خدمات محلی
public class LocalService
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public LocalServiceCategory Category { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ProviderName { get; private set; } = string.Empty;
    public string ContactPhone { get; private set; } = string.Empty;
    public string WorkingHours { get; private set; } = string.Empty;
    
    public double AverageRating { get; private set; }
    public int RatingCount { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    public Building Building { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    public IReadOnlyCollection<LocalServiceRating> Ratings => _ratings.AsReadOnly();
    private readonly List<LocalServiceRating> _ratings = new();

    private LocalService() { }

    public static LocalService Create(
        Guid buildingId, Guid createdByUserId, LocalServiceCategory category,
        string title, string description, string providerName,
        string contactPhone, string workingHours)
    {
        return new LocalService
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            CreatedByUserId = createdByUserId,
            Category = category,
            Title = title,
            Description = description,
            ProviderName = providerName,
            ContactPhone = contactPhone,
            WorkingHours = workingHours,
            AverageRating = 0,
            RatingCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void UpdateRating(double averageRating, int ratingCount)
    {
        AverageRating = averageRating;
        RatingCount = ratingCount;
    }
}

// امتیاز به خدمات محلی
public class LocalServiceRating
{
    public Guid Id { get; private set; }
    public Guid LocalServiceId { get; private set; }
    public Guid UserId { get; private set; }
    public int Score { get; private set; }   // 1 تا 5
    public DateTime CreatedAt { get; private set; }

    public LocalService LocalService { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private LocalServiceRating() { }

    public static LocalServiceRating Create(Guid localServiceId, Guid userId, int score)
    {
        if (score < 1 || score > 5)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 1 and 5");

        return new LocalServiceRating
        {
            Id = Guid.NewGuid(),
            LocalServiceId = localServiceId,
            UserId = userId,
            Score = score,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateScore(int score)
    {
        if (score < 1 || score > 5)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 1 and 5");
        Score = score;
    }
}

// خرید گروهی
public class GroupBuying
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string OrganizerFullName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public int MinimumQuantity { get; private set; }
    public decimal Price { get; private set; }
    public DateTime Deadline { get; private set; }
    public string? ImageUrl { get; private set; }
    public int Block { get; private set; }
    public int Floor { get; private set; }
    public int UnitNumber { get; private set; }
    public string ContactPhone { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Building Building { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    public IReadOnlyCollection<GroupBuyingParticipant> Participants => _participants.AsReadOnly();
    private readonly List<GroupBuyingParticipant> _participants = new();

    private GroupBuying() { }

    public static GroupBuying Create(
        Guid buildingId, Guid createdByUserId, string organizerFullName,
        string title, int minimumQuantity, decimal price, DateTime deadline,
        int block, int floor, int unitNumber, string contactPhone)
    {
        return new GroupBuying
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            CreatedByUserId = createdByUserId,
            OrganizerFullName = organizerFullName,
            Title = title,
            MinimumQuantity = minimumQuantity,
            Price = price,
            Deadline = deadline,
            Block = block,
            Floor = floor,
            UnitNumber = unitNumber,
            ContactPhone = contactPhone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, int minimumQuantity, decimal price, DateTime deadline,
        string? imageUrl, int block, int floor, int unitNumber, string contactPhone)
    {
        Title = title;
        MinimumQuantity = minimumQuantity;
        Price = price;
        Deadline = deadline;
        ImageUrl = imageUrl;
        Block = block;
        Floor = floor;
        UnitNumber = unitNumber;
        ContactPhone = contactPhone;
        UpdatedAt = DateTime.UtcNow;
    }
    public void AddParticipant(Guid userId)
    {
        if (_participants.Any(p => p.UserId == userId))
            throw new InvalidOperationException("شما قبلاً به این کمپین پیوسته‌اید.");

        _participants.Add(GroupBuyingParticipant.Create(Id, userId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveParticipant(Guid userId)
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId)
                          ?? throw new InvalidOperationException("شما عضو این کمپین نیستید.");

        _participants.Remove(participant);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsDeadlineExpired => DateTime.UtcNow > Deadline;

    public bool IsOwner(Guid userId) => CreatedByUserId == userId;
}

// شرکت‌کننده در خرید گروهی
public class GroupBuyingParticipant
{
    public Guid Id { get; private set; }
    public Guid GroupBuyingId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAt { get; private set; }

    public GroupBuying GroupBuying { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private GroupBuyingParticipant() { }

    public static GroupBuyingParticipant Create(Guid groupBuyingId, Guid userId)
    {
        return new GroupBuyingParticipant
        {
            Id = Guid.NewGuid(),
            GroupBuyingId = groupBuyingId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };
    }
}
