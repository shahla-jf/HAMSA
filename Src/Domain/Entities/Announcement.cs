using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

// اعلان
public class Announcement
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid? CreatedByUserId { get; private set; }  // null یعنی سیستم ایجاد کرده
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AnnouncementPriority Priority { get; private set; }
    public AnnouncementSource Source { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Building Building { get; private set; } = null!;
    public User? CreatedByUser { get; private set; }

    public IReadOnlyCollection<AnnouncementRead> ReadRecords => _readRecords.AsReadOnly();
    private readonly List<AnnouncementRead> _readRecords = new();

    private Announcement() { }

    public static Announcement CreateByManager(
        Guid buildingId, Guid createdByUserId, string title, string description, AnnouncementPriority priority)
    {
        return new Announcement
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            CreatedByUserId = createdByUserId,
            Title = title,
            Description = description,
            Priority = priority,
            Source = AnnouncementSource.Manager,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Announcement CreateBySystem(Guid buildingId, string title, string description)
    {
        return new Announcement
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            CreatedByUserId = null,
            Title = title,
            Description = description,
            Priority = AnnouncementPriority.Normal,
            Source = AnnouncementSource.System,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// وضعیت خوانده شدن اعلان توسط کاربر
public class AnnouncementRead
{
    public Guid Id { get; private set; }
    public Guid AnnouncementId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ReadAt { get; private set; }

    public Announcement Announcement { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private AnnouncementRead() { }

    public static AnnouncementRead Create(Guid announcementId, Guid userId)
    {
        return new AnnouncementRead
        {
            Id = Guid.NewGuid(),
            AnnouncementId = announcementId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        };
    }
}

// نوتیفیکیشن درون‌برنامه‌ای
public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = null!;

    private Notification() { }

    public static Notification Create(Guid userId, string title, string body)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Body = body,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
