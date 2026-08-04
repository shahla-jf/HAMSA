using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HAMSA.Infrastructure.Persistence.Repositories;

public class OtpRepository : Repository<OtpCode>, IOtpRepository
{
    public OtpRepository(AppDbContext context) : base(context) { }

    public async Task<OtpCode?> GetValidOtpAsync(string phoneNumber, string code)
        => await _dbSet.FirstOrDefaultAsync(o =>
            o.PhoneNumber == phoneNumber &&
            o.Code == code &&
            !o.IsUsed &&
            o.ExpiresAt > DateTime.UtcNow);

    public async Task InvalidatePreviousAsync(string phoneNumber)
    {
        var previousOtps = await _dbSet
            .Where(o => o.PhoneNumber == phoneNumber && !o.IsUsed)
            .ToListAsync();

        foreach (var otp in previousOtps)
            otp.MarkAsUsed();

        await _context.SaveChangesAsync();
    }
}



public class RevokedTokenRepository : Repository<RevokedToken>, IRevokedTokenRepository
{
    public RevokedTokenRepository(AppDbContext context) : base(context) { }

    public async Task<bool> IsRevokedAsync(string jti)
    {
        return await _dbSet.AnyAsync(t => t.Jti == jti);
    }
}