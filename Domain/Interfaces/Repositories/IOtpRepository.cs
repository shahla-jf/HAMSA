using HAMSA.Domain.Entities;

namespace HAMSA.Domain.Interfaces.Repositories;

public interface IOtpRepository : IRepository<OtpCode>
{
    Task<OtpCode?> GetValidOtpAsync(string phoneNumber, string code);
    Task InvalidatePreviousAsync(string phoneNumber);
}


public interface IRevokedTokenRepository : IRepository<RevokedToken>
{
    Task<bool> IsRevokedAsync(string jti);
}