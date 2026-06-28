namespace HAMSA.Domain.Interfaces.Services;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string code);
}

public interface IJwtService
{
    string GenerateToken(Guid userId, string phoneNumber);
    Guid? ValidateToken(string token);
}
