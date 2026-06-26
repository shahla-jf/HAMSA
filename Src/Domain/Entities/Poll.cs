using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Entities;

// رای‌گیری
public class Poll
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public PollAudience Audience { get; private set; }
    public DateTime Deadline { get; private set; }
    public bool IsActive => DateTime.UtcNow <= Deadline;
    public DateTime CreatedAt { get; private set; }

    public Building Building { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;

    public IReadOnlyCollection<PollOption> Options => _options.AsReadOnly();
    private readonly List<PollOption> _options = new();

    private Poll() { }

    public static Poll Create(
        Guid buildingId, Guid createdByUserId,
        string title, string? description, PollAudience audience, DateTime deadline)
    {
        return new Poll
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            CreatedByUserId = createdByUserId,
            Title = title,
            Description = description,
            Audience = audience,
            Deadline = deadline,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// گزینه‌های رای‌گیری
public class PollOption
{
    public Guid Id { get; private set; }
    public Guid PollId { get; private set; }
    public string Text { get; private set; } = string.Empty;

    public Poll Poll { get; private set; } = null!;

    public IReadOnlyCollection<PollVote> Votes => _votes.AsReadOnly();
    private readonly List<PollVote> _votes = new();

    private PollOption() { }

    public static PollOption Create(Guid pollId, string text)
    {
        return new PollOption
        {
            Id = Guid.NewGuid(),
            PollId = pollId,
            Text = text
        };
    }
}

// رای کاربر
public class PollVote
{
    public Guid Id { get; private set; }
    public Guid PollOptionId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime VotedAt { get; private set; }

    public PollOption PollOption { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private PollVote() { }

    public static PollVote Create(Guid pollOptionId, Guid userId)
    {
        return new PollVote
        {
            Id = Guid.NewGuid(),
            PollOptionId = pollOptionId,
            UserId = userId,
            VotedAt = DateTime.UtcNow
        };
    }
}
