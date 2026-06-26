using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? ProfileImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // روابط
    public IReadOnlyCollection<BuildingMembership> Memberships => _memberships.AsReadOnly();
    private readonly List<BuildingMembership> _memberships = new();

    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();
    private readonly List<Notification> _notifications = new();

    public IReadOnlyCollection<ChatGroupMember> ChatGroupMemberships => _chatGroupMemberships.AsReadOnly();
    private readonly List<ChatGroupMember> _chatGroupMemberships = new();

    private User() { }

    public static User Create(string phoneNumber)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phoneNumber,
            FirstName = string.Empty,
            LastName = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string firstName, string lastName, string phoneNumber, string? profileImageUrl = null)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        ProfileImageUrl = profileImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
