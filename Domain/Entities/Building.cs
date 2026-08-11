namespace HAMSA.Domain.Entities;

public class Building
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int BlockCount { get; private set; }
    public int FloorCount { get; private set; }
    public int UnitCount { get; private set; }
    public string PostalCode { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public bool HasGym { get; private set; }
    public bool HasPool { get; private set; }
    public bool HasMeetingHall { get; private set; }
    public bool HasRoofGarden { get; private set; }
    public string FacilitiesPhone { get; private set; } = string.Empty;
    public string ManagementPhone { get; private set; } = string.Empty;
    public string LobbyPhone { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }

    // هزینه‌های مشاعات ماهانه
    public decimal SharedElectricityCost { get; private set; }
    public bool IsElectricityPaid { get; private set; }
    
    public decimal SharedWaterCost { get; private set; }
    public bool IsWaterPaid { get; private set; }

    public decimal CleaningCost { get; private set; }
    public bool IsCleaningPaid { get; private set; }
    
    public decimal ElevatorCost { get; private set; }
    public bool IsElevatorPaid { get; private set; }
    
    // تنظیمات مالی
    public decimal LatePenaltyPercent { get; private set; }   // درصد جریمه تاخیر

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // روابط
    public IReadOnlyCollection<Unit> Units => _units.AsReadOnly();
    private readonly List<Unit> _units = new();

    public IReadOnlyCollection<BuildingMembership> Memberships => _memberships.AsReadOnly();
    private readonly List<BuildingMembership> _memberships = new();

    public IReadOnlyCollection<BuildingManagerHistory> ManagerHistory => _managerHistory.AsReadOnly();
    private readonly List<BuildingManagerHistory> _managerHistory = new();

    public IReadOnlyCollection<Announcement> Announcements => _announcements.AsReadOnly();
    private readonly List<Announcement> _announcements = new();

    public IReadOnlyCollection<ChatGroup> ChatGroups => _chatGroups.AsReadOnly();
    private readonly List<ChatGroup> _chatGroups = new();

    private Building() { }

    public static Building Create(
        string name, int blockCount, int floorCount, int unitCount,
        string postalCode, string address, double latitude, double longitude,
        bool hasGym, bool hasPool, bool hasMeetingHall, bool hasRoofGarden,
        string facilitiesPhone, string managementPhone, string lobbyPhone,
        string? imageUrl = null)
    {
        return new Building
        {
            Id = Guid.NewGuid(),
            Name = name,
            BlockCount = blockCount,
            FloorCount = floorCount,
            UnitCount = unitCount,
            PostalCode = postalCode,
            Address = address,
            Latitude = latitude,
            Longitude = longitude,
            HasGym = hasGym,
            HasPool = hasPool,
            HasMeetingHall = hasMeetingHall,
            HasRoofGarden = hasRoofGarden,
            FacilitiesPhone = facilitiesPhone,
            ManagementPhone = managementPhone,
            LobbyPhone = lobbyPhone,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name, int blockCount, int floorCount, int unitCount,
        string postalCode, string address, double latitude, double longitude,
        bool hasGym, bool hasPool, bool hasMeetingHall, bool hasRoofGarden,
        string facilitiesPhone, string managementPhone, string lobbyPhone,
        string? imageUrl = null)
    {
        Name = name;
        BlockCount = blockCount;
        FloorCount = floorCount;
        UnitCount = unitCount;
        PostalCode = postalCode;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        HasGym = hasGym;
        HasPool = hasPool;
        HasMeetingHall = hasMeetingHall;
        HasRoofGarden = hasRoofGarden;
        FacilitiesPhone = facilitiesPhone;
        ManagementPhone = managementPhone;
        LobbyPhone = lobbyPhone;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSharedCosts(
        decimal electricity, bool isElectricityPaid, 
        decimal water, bool isWaterPaid, 
        decimal cleaning, bool isCleaningPaid, 
        decimal elevator, bool isElevatorPaid)
    {
        SharedElectricityCost = electricity;
        IsElectricityPaid = isElectricityPaid;
        
        SharedWaterCost = water;
        IsWaterPaid = isWaterPaid;
        
        CleaningCost = cleaning;
        IsCleaningPaid = isCleaningPaid;
        
        ElevatorCost = elevator;
        IsElevatorPaid = isElevatorPaid;
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLatePenalty(decimal percent)
    {
        LatePenaltyPercent = percent;
        UpdatedAt = DateTime.UtcNow;
    }
}
