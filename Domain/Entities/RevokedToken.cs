namespace HAMSA.Domain.Entities;

public class RevokedToken
{
    public Guid Id { get; private set; }
    public string Jti { get; private set; } = string.Empty; // JWT ID (شناسه یکتای توکن)
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; } // همون انقضای اصلی توکن
    public DateTime RevokedAt { get; private set; }

    private RevokedToken() { }

    public static RevokedToken Create(string jti, Guid userId, DateTime expiresAt)
    {
        return new RevokedToken
        {
            Id = Guid.NewGuid(),
            Jti = jti,
            UserId = userId,
            ExpiresAt = expiresAt,
            RevokedAt = DateTime.UtcNow
        };
    }
}