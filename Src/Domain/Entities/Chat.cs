namespace HAMSA.Domain.Entities;

// گروه چت
public class ChatGroup
{
    public Guid Id { get; private set; }
    public Guid BuildingId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsMandatory { get; private set; }    // مثل "ساختمان شما" که اجباریه
    public bool IsDefault { get; private set; }      // گروه‌های پیش‌فرض ساختمان
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Building Building { get; private set; } = null!;
    public User? CreatedByUser { get; private set; }

    public IReadOnlyCollection<ChatGroupMember> Members => _members.AsReadOnly();
    private readonly List<ChatGroupMember> _members = new();

    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();
    private readonly List<ChatMessage> _messages = new();

    private ChatGroup() { }

    public static ChatGroup CreateDefault(Guid buildingId, string name, bool isMandatory)
    {
        return new ChatGroup
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            Name = name,
            IsMandatory = isMandatory,
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ChatGroup CreateByUser(Guid buildingId, Guid createdByUserId, string name)
    {
        return new ChatGroup
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            Name = name,
            IsMandatory = false,
            IsDefault = false,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// عضو گروه چت
public class ChatGroupMember
{
    public Guid Id { get; private set; }
    public Guid ChatGroupId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAt { get; private set; }

    public ChatGroup ChatGroup { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private ChatGroupMember() { }

    public static ChatGroupMember Create(Guid chatGroupId, Guid userId)
    {
        return new ChatGroupMember
        {
            Id = Guid.NewGuid(),
            ChatGroupId = chatGroupId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };
    }
}

// پیام چت
public class ChatMessage
{
    public Guid Id { get; private set; }
    public Guid ChatGroupId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? FileUrl { get; private set; }
    public DateTime SentAt { get; private set; }

    public ChatGroup ChatGroup { get; private set; } = null!;
    public User Sender { get; private set; } = null!;

    private ChatMessage() { }

    public static ChatMessage Create(Guid chatGroupId, Guid senderId, string content, string? fileUrl = null)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatGroupId = chatGroupId,
            SenderId = senderId,
            Content = content,
            FileUrl = fileUrl,
            SentAt = DateTime.UtcNow
        };
    }
}
