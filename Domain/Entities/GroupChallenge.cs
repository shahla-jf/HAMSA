using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

// چالش گروهی هفتگی
public class GroupChallenge
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public string? Title { get; private set; }           // توسط AI پر می‌شود
    public string? Description { get; private set; }     // توسط AI پر می‌شود
    public GroupChallengeStatus Status { get; private set; }
    public DateTime RegistrationOpenDate { get; private set; }   // شنبه
    public DateTime RegistrationCloseDate { get; private set; }  // دوشنبه
    public DateTime ChallengeDeadline { get; private set; }      // جمعه
    public DateTime CreatedAt { get; private set; }

    public Building Building { get; private set; } = null!;

    public IReadOnlyCollection<GroupChallengeParticipant> Participants => _participants.AsReadOnly();
    private readonly List<GroupChallengeParticipant> _participants = new();

    private GroupChallenge() { }

    public static GroupChallenge Create(
        Guid buildingId,
        DateTime registrationOpenDate,
        DateTime registrationCloseDate,
        DateTime challengeDeadline)
    {
        return new GroupChallenge
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            Status = GroupChallengeStatus.RegistrationOpen,
            RegistrationOpenDate = registrationOpenDate,
            RegistrationCloseDate = registrationCloseDate,
            ChallengeDeadline = challengeDeadline,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetAiGeneratedChallenge(string title, string description)
    {
        Title = title;
        Description = description;
        Status = GroupChallengeStatus.ChallengeActive;
    }

    public void Complete()
    {
        Status = GroupChallengeStatus.Completed;
    }
    
    public void AddParticipant(GroupChallengeParticipant participant)
    {
        if (_participants.Any(p => p.UserId == participant.UserId))
            throw new InvalidOperationException("کاربر قبلا ثبت‌نام کرده است.");

        _participants.Add(participant);
    }
}

// شرکت‌کننده در چالش
public class GroupChallengeParticipant
{
    public Guid Id { get; private set; }
    public Guid GroupChallengeId { get; private set; }
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public int Age { get; private set; }
    public Gender Gender { get; private set; }
    public string? SportsBackground { get; private set; }   // سابقه ورزشی
    public bool IsCompleted { get; private set; }           // "انجام دادم" را زده یا نه
    public DateTime RegisteredAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public GroupChallenge GroupChallenge { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private GroupChallengeParticipant() { }

    public static GroupChallengeParticipant Create(
        Guid groupChallengeId, Guid userId,
        string fullName, int age, Gender gender, string? sportsBackground)
    {
        return new GroupChallengeParticipant
        {
            Id = Guid.NewGuid(),
            GroupChallengeId = groupChallengeId,
            UserId = userId,
            FullName = fullName,
            Age = age,
            Gender = gender,
            SportsBackground = sportsBackground,
            IsCompleted = false,
            RegisteredAt = DateTime.UtcNow
        };
    }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }
}
