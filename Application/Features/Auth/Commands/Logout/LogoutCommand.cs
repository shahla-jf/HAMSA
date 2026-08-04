using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string Jti, Guid UserId, DateTime ExpiresAt);

public record LogoutResult(bool Success, string Message);


public class LogoutHandler
{
    private readonly IRevokedTokenRepository _revokedTokenRepository;

    public LogoutHandler(IRevokedTokenRepository revokedTokenRepository)
    {
        _revokedTokenRepository = revokedTokenRepository;
    }

    public async Task<LogoutResult> HandleAsync(LogoutCommand command)
    {
        var revoked = RevokedToken.Create(command.Jti, command.UserId, command.ExpiresAt);
        await _revokedTokenRepository.AddAsync(revoked);
        await _revokedTokenRepository.SaveChangesAsync();

        return new LogoutResult(true, "خروج با موفقیت انجام شد");
    }
}