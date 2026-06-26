using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

// گزارش خرابی
public class RepairReport
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid ReportedByUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RepairPriority Priority { get; private set; }
    public RepairStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<RepairReportMedia> MediaFiles => _mediaFiles.AsReadOnly();
    private readonly List<RepairReportMedia> _mediaFiles = new();

    public Building Building { get; private set; } = null!;
    public User ReportedByUser { get; private set; } = null!;

    private RepairReport() { }

    public static RepairReport Create(
        Guid buildingId, Guid reportedByUserId,
        string title, string description, RepairPriority priority)
    {
        return new RepairReport
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            ReportedByUserId = reportedByUserId,
            Title = title,
            Description = description,
            Priority = priority,
            Status = RepairStatus.Registered,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(RepairStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}

// عکس یا فیلم ضمیمه گزارش خرابی
public class RepairReportMedia
{
    public Guid Id { get; private set; }
    public Guid RepairReportId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public bool IsVideo { get; private set; }

    public RepairReport RepairReport { get; private set; } = null!;

    private RepairReportMedia() { }

    public static RepairReportMedia Create(Guid repairReportId, string url, bool isVideo)
    {
        return new RepairReportMedia
        {
            Id = Guid.NewGuid(),
            RepairReportId = repairReportId,
            Url = url,
            IsVideo = isVideo
        };
    }
}
