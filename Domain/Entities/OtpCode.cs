namespace HAMSA.Domain.Entities;

public class OtpCode
{
    public Guid Id { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private OtpCode() { }

    public static OtpCode Create(string phoneNumber)
    {
        return new OtpCode
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phoneNumber,
            Code = "11111",
            // Code = new Random().Next(10000, 99999).ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(2),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsValid() => !IsUsed && DateTime.UtcNow <= ExpiresAt;

    public void MarkAsUsed() => IsUsed = true;
}