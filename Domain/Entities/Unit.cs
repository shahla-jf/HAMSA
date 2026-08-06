using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

// واحد مسکونی
public class Unit
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public int Block { get; private set; }
    public int Floor { get; private set; }
    public int UnitNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // روابط
    public Building Building { get; private set; } = null!;

    public IReadOnlyCollection<BuildingMembership> Memberships => _memberships.AsReadOnly();
    private readonly List<BuildingMembership> _memberships = new();

    public IReadOnlyCollection<Charge> Charges => _charges.AsReadOnly();
    private readonly List<Charge> _charges = new();

    private Unit() { }

    public static Unit Create(Guid buildingId, int block, int floor, int unitNumber)
    {
        return new Unit
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            Block = block,
            Floor = floor,
            UnitNumber = unitNumber,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(int block, int floor, int unitNumber)
    {
        Block = block;
        Floor = floor;
        UnitNumber = unitNumber;
    }
}

// عضویت کاربر در ساختمان (مالک یا مستاجر بودن در یه واحد خاص)
public class BuildingMembership
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid? UnitId { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsResident { get; private set; }    // ساکن است یا موجر (برای مالک)
    public bool IsPrimary { get; private set; }      // اصلی یا فرعی بودن عضویت
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? InviteCode { get; private set; } // کد دعوت مستاجر
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastSelectedAt { get; private set; }

    // روابط
    public User User { get; private set; } = null!;
    public Building Building { get; private set; } = null!;
    public Unit? Unit { get; private set; } = null!;

    private BuildingMembership() { }

    public static BuildingMembership Create(
        Guid userId, Guid buildingId, Guid? unitId,
        UserRole role, bool isResident,
        DateTime startDate, bool isPrimary, DateTime? endDate)
    {
        return new BuildingMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BuildingId = buildingId,
            UnitId = unitId,
            Role = role,
            IsResident = isResident,
            IsPrimary = isPrimary,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void GenerateInviteCode()
    {
        InviteCode = Guid.NewGuid().ToString("N")[..8].ToUpper();
    }

    public void UpdateStartDate(DateTime startDate)
    {
        StartDate = startDate;
    }

    public void SetEndDate(DateTime endDate)
    {
        EndDate = endDate;
        IsActive = endDate > DateTime.UtcNow ? true : false;
    }

    public void Deactivate()
    {
        IsActive = false;
        EndDate = DateTime.UtcNow;
    }
    
    public void MarkAsLastSelected()
    {
        LastSelectedAt = DateTime.UtcNow;
    }
}

// تاریخچه مدیران ساختمان
public class BuildingManagerHistory
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsCurrent { get; private set; }

    public Building Building { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private BuildingManagerHistory() { }

    public static BuildingManagerHistory Create(Guid buildingId, Guid userId)
    {
        return new BuildingManagerHistory
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            UserId = userId,
            StartDate = DateTime.UtcNow,
            IsCurrent = true
        };
    }

    public void End()
    {
        EndDate = DateTime.UtcNow;
        IsCurrent = false;
    }
}
